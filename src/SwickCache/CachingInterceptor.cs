using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CachingInterceptor : IAsyncInterceptor, ICacheEntryInvalidator
    {
        private readonly IDistributedCache _cache;
        private readonly IOptionsMonitor<CachingOptions> _options;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly ICacheSerializer _serializer;
        private readonly CacheExpirationProvider _expirationProvider;
        private readonly ILogger<CachingInterceptor> _logger;

        public CachingInterceptor(
            IDistributedCache cache,
            ICacheSerializer serializer,
            IOptionsMonitor<CachingOptions> options,
            ICacheKeyProvider keyProvider,
            CacheExpirationProvider expirationProvider,
            ILogger<CachingInterceptor> logger)
        {
            _cache = cache;
            _serializer = serializer;
            _options = options;
            _keyProvider = keyProvider;
            _expirationProvider = expirationProvider;
            _logger = logger;
        }

        private static readonly MethodInfo HandleSyncMethodInfo = typeof(CachingInterceptor)
            .GetMethod(nameof(HandleSynchronous), BindingFlags.Static | BindingFlags.NonPublic);

        private delegate void SynchronousHandler(IInvocation invocation);

        private static ConcurrentDictionary<Type, SynchronousHandler> _handlers = new ConcurrentDictionary<Type, SynchronousHandler>();

        public void InterceptSynchronous(IInvocation invocation)
        {
            if (invocation.Method.ReturnType != null)
            {
                var handler = _handlers.GetOrAdd(invocation.Method.ReturnType, t => (SynchronousHandler)HandleSyncMethodInfo.MakeGenericMethod(t).CreateDelegate(typeof(SynchronousHandler)));

                handler(invocation);
            }
        }

        private void HandleSynchronous<TResult>(IInvocation invocation)
        {
            var result = InterceptAsynchronous(invocation, new Proceed<TResult>(invocation), isAsync: false);

            if (!result.IsCompleted)
            {
                throw new InvalidOperationException("Synchronous operations must not result in asynchronous actions.");
            }

            invocation.ReturnValue = result.Result;
        }

        public void InterceptAsynchronous(IInvocation invocation)
            => invocation.Proceed();

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InterceptAsynchronous(invocation, new Proceed<TResult>(invocation), isAsync: true).AsTask();
        }

        private async ValueTask<TResult> InterceptAsynchronous<TResult>(IInvocation invocation, Proceed<TResult> proceed, bool isAsync)
        {
            if (!_options.CurrentValue.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            }

            var key = GetCacheKey(invocation);

            _logger.LogDebug("Checking for cached value for '{Key}'", key);

            var cached = await GetAsync(key, isAsync).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);

                return GetValue(cached);
            }

            _logger.LogDebug("Found cached value for '{Key}'", key);

            var result = await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            var expiration = _expirationProvider.GetExpiration(invocation.Method, result);

            if (expiration.HasValue)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration.Value,
                };

                await SetAsync(key, GetBytes(result), options, isAsync).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("No expiration is defined for {Invocation}", invocation);
                await SetAsync(key, GetBytes(result), null, isAsync).ConfigureAwait(false);
            }

            return result;

            byte[] GetBytes(TResult input)
            {
                if (input is byte[] b)
                {
                    return b;
                }
                else
                {
                    return _serializer.GetBytes<TResult>(input);
                }
            }

            TResult GetValue(byte[] input)
            {
                if (input is TResult r)
                {
                    return r;
                }
                else
                {
                    return _serializer.GetValue<TResult>(input);
                }
            }
        }

        public Task InvalidateAsync(IInvocation invocation)
        {
            var key = GetCacheKey(invocation);

            _logger.LogTrace("Removing cached value for {Key}", key);

            return _cache.RemoveAsync(key);
        }

        private string GetCacheKey(IInvocation invocation) => _keyProvider.GetKey(invocation.Method, invocation.Arguments);

        private async ValueTask SetAsync(string key, byte[] result, DistributedCacheEntryOptions options, bool isAsync)
        {
            if (isAsync)
            {
                await _cache.SetAsync(key, _serializer.GetBytes(result), options).ConfigureAwait(false);
            }
            else
            {
                _cache.Set(key, _serializer.GetBytes(result), options);
            }
        }
        private async ValueTask<byte[]> GetAsync(string key, bool isAsync)
        {
            try
            {
                return isAsync ? await _cache.GetAsync(key).ConfigureAwait(false) : _cache.Get(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while accessing cached value.");
                return null;
            }
        }
    }
}
