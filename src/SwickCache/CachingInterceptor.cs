using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
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

        private delegate void SynchronousHandler(CachingInterceptor interceptor, IInvocation invocation);

        private static readonly ConcurrentDictionary<Type, SynchronousHandler> _handlers = new ConcurrentDictionary<Type, SynchronousHandler>();

        public void InterceptSynchronous(IInvocation invocation)
        {
            if (invocation.Method.ReturnType != null)
            {
                var handler = _handlers.GetOrAdd(invocation.Method.ReturnType, t => (SynchronousHandler)HandleSyncMethodInfo.MakeGenericMethod(t).CreateDelegate(typeof(SynchronousHandler)));

                handler(this, invocation);
            }
        }

        private static void HandleSynchronous<TResult>(CachingInterceptor interceptor, IInvocation invocation)
        {
            var result = interceptor.InterceptInternalAsync(invocation, new Proceed<TResult>(invocation), isAsync: false, default);

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
            var cancellationToken = invocation.Arguments.OfType<CancellationToken>().FirstOrDefault();

            invocation.ReturnValue = InterceptInternalAsync(invocation, new Proceed<TResult>(invocation), isAsync: true, cancellationToken).AsTask();
        }

        private async ValueTask<TResult> InterceptInternalAsync<TResult>(IInvocation invocation, Proceed<TResult> proceed, bool isAsync, CancellationToken token)
        {
            if (!_options.CurrentValue.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            }

            var key = GetCacheKey(invocation);

            _logger.LogDebug("Checking for cached value for '{Key}'", key);

            var cached = await GetAsync(key, isAsync, token).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);

                return _serializer.GetValue<TResult>(cached);
            }

            _logger.LogDebug("Did not find cached value for '{Key}'", key);

            var result = await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            var expiration = _expirationProvider.GetExpiration(invocation.Method, result);

            if (expiration.HasValue)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration.Value,
                };

                await SetAsync(key, _serializer.GetBytes(result), options, isAsync, token).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("No expiration is defined for {Invocation}", invocation);
                await SetAsync(key, _serializer.GetBytes(result), null, isAsync, token).ConfigureAwait(false);
            }

            return result;
        }

        public Task InvalidateAsync(IInvocation invocation)
        {
            var key = GetCacheKey(invocation);

            _logger.LogTrace("Removing cached value for {Key}", key);

            return _cache.RemoveAsync(key);
        }

        private string GetCacheKey(IInvocation invocation) => _keyProvider.GetKey(invocation.Method, invocation.Arguments);

        private async ValueTask SetAsync(string key, byte[] result, DistributedCacheEntryOptions options, bool isAsync, CancellationToken token)
        {
            var bytes = _serializer.GetBytes(result);

            try
            {
                if (isAsync)
                {
                    await _cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
                }
                else
                {
                    _cache.Set(key, bytes, options);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while writing to cache.");
            }
        }

        private async ValueTask<byte[]> GetAsync(string key, bool isAsync, CancellationToken token)
        {
            try
            {
                return isAsync ? await _cache.GetAsync(key, token).ConfigureAwait(false) : _cache.Get(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while getting cached value.");
                return null;
            }
        }
    }
}
