﻿using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CachingInterceptorHandler<T> : ICachingInterceptorHandler, IResultTransformer<T>
    {
        private readonly CachingInterceptorHandlerOptions _cacheAccessor;
        private readonly ICacheSerializer _serializer;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly IResultTransformer<T> _transformer;
        private readonly IOptionsMonitor<CachingOptions> _options;
        private readonly ILogger<CachingInterceptor> _logger;

        public CachingInterceptorHandler(
            IOptions<CachingInterceptorHandlerOptions> cacheAccessor,
            ICacheSerializer serializer,
            ICacheKeyProvider keyProvider,
            IOptionsMonitor<CachingOptions> options,
            IServiceProvider services,
            ILogger<CachingInterceptor> logger)
        {
            _cacheAccessor = cacheAccessor.Value;
            _serializer = serializer;
            _keyProvider = keyProvider;
            _transformer = services.GetService<IResultTransformer<T>>() ?? this;
            _options = options;
            _logger = logger;
        }

        public void Invalidate(IInvocation invocation, MethodType methodType)
        {
            var result = InvalidateInternal(invocation, isAsync: methodType != MethodType.Synchronous);

            invocation.ReturnValue = GetResult(methodType, result);

        }

        public void Intercept(IInvocation invocation, MethodType methodType, CancellationToken token)
        {
            var result = InterceptInternalAsync(invocation, methodType, token);

            invocation.ReturnValue = GetResult(methodType, result);
        }

        private object GetResult(MethodType methodType, ValueTask<T> result)
            => methodType switch
            {
                MethodType.Task => result.AsTask(),
                MethodType.ValueTask => result,
                MethodType.Synchronous => UnwrapValueTask(result),
                _ => throw new NotSupportedException($"Unexpected MethodType '{methodType}'"),
            };

        private static T UnwrapValueTask(ValueTask<T> result)
            => result.IsCompleted
                ? result.Result
                : throw new InvalidOperationException("Synchronous operations must not result in asynchronous actions.");


        private async ValueTask<T> InvalidateInternal(IInvocation invocation, bool isAsync)
        {
            var cache = _cacheAccessor.GetCache(invocation.TargetType, invocation.Method);
            var key = _keyProvider.GetKey(invocation.Method, invocation.Arguments);

            _logger.LogTrace("Removing cached value for {Key}", key);

            if (isAsync)
            {
                await cache.RemoveAsync(key);
            }
            else
            {
                cache.Remove(key);
            }

            _logger.LogTrace("Removed cached value for {Key}", key);

            return default;
        }

        public async ValueTask<T> InterceptInternalAsync(IInvocation invocation, MethodType methodType, CancellationToken token)
        {
            var proceed = new Proceed<T>(invocation, methodType);
            var isAsync = methodType != MethodType.Synchronous;
            var cachingOptions = _options.CurrentValue;

            if (!cachingOptions.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed.InvokeAsync().ConfigureAwait(false);
            }

            var key = _keyProvider.GetKey(invocation.Method, invocation.Arguments);
            var cache = _cacheAccessor.GetCache(invocation.Method.DeclaringType, invocation.Method);

            _logger.LogDebug("Checking for cached value for '{Key}'", key);

            var cached = await GetAsync(cache, key, isAsync, token).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);

                try
                {
                    return _serializer.GetValue<T>(cached);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error deserializing {Key}. Recaching new value.", key);
                }
            }
            else
            {
                _logger.LogDebug("Did not find cached value for '{Key}'", key);
            }

            var result = await ProceedAndTransformAsync(proceed, token);

            if (result is null)
            {
                _logger.LogTrace("Null response returned and cannot be cached.");
                return result;
            }

            if (!ShouldCache(cachingOptions, result))
            {
                _logger.LogTrace("Not caching {Key}", key);
                return result;
            }

            var bytes = GetBytes(result);

            if (bytes is null)
            {
                return result;
            }

            var options = new DistributedCacheEntryOptions();

            foreach (var handler in cachingOptions.InternalHandlers)
            {
                handler.ConfigureEntryOptions(typeof(T), invocation.Method, result, options);
            }

            await SetAsync(cache, key, bytes, options, isAsync, token).ConfigureAwait(false);

            _logger.LogDebug("Cached result for '{Key}'", key);

            return await _transformer.ResetAsync(result, token).ConfigureAwait(false);
        }

        private async ValueTask<T> ProceedAndTransformAsync(Proceed<T> proceed, CancellationToken token)
        {
            var result = await proceed.InvokeAsync().ConfigureAwait(false);

            return await _transformer.TransformAsync(result, token).ConfigureAwait(false);
        }

        private bool ShouldCache(CachingOptions options, T obj)
        {
            foreach (var handler in options.InternalHandlers)
            {
                if (!handler.ShouldCache(obj))
                {
                    return false;
                }
            }

            return true;
        }

        private byte[] GetBytes(T result)
        {
            try
            {
                return _serializer.GetBytes(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error serializing");
                return null;
            }
        }

        private async ValueTask SetAsync(IDistributedCache cache, string key, byte[] bytes, DistributedCacheEntryOptions options, bool isAsync, CancellationToken token)
        {
            try
            {
                if (isAsync)
                {
                    await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
                }
                else
                {
                    cache.Set(key, bytes, options);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while writing to cache.");
            }
        }

        private async ValueTask<byte[]> GetAsync(IDistributedCache cache, string key, bool isAsync, CancellationToken token)
        {
            try
            {
                return isAsync ? await cache.GetAsync(key, token).ConfigureAwait(false) : cache.Get(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while getting cached value.");
                return null;
            }
        }

        ValueTask<T> IResultTransformer<T>.TransformAsync(T input, CancellationToken token) => new ValueTask<T>(input);

        ValueTask<T> IResultTransformer<T>.ResetAsync(T input, CancellationToken token) => new ValueTask<T>(input);
    }
}
