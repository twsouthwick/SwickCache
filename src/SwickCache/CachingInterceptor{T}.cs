using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal interface ICachingInterceptor
    {
        void Intercept(IInvocation invocation, bool isAsync, CancellationToken token);
    }

    internal class CachingInterceptor<T> : ICachingInterceptor
    {
        private readonly ICacheSerializer<T> _serializer;
        private readonly IDistributedCache _cache;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly IOptionsSnapshot<CachingOptions> _options;
        private readonly ILogger<CachingInterceptor> _logger;

        public CachingInterceptor(
            ICacheSerializer<T> serializer,
            IDistributedCache cache,
            ICacheKeyProvider keyProvider,
            IOptionsSnapshot<CachingOptions> options,
            ILogger<CachingInterceptor> logger)
        {
            _serializer = serializer;
            _cache = cache;
            _keyProvider = keyProvider;
            _options = options;
            _logger = logger;
        }

        public void Intercept(IInvocation invocation, bool isAsync, CancellationToken token)
        {
            var result = InterceptInternalAsync(invocation, isAsync, token);

            if (isAsync)
            {
                invocation.ReturnValue = result.AsTask();
            }
            else if (result.IsCompleted)
            {
                invocation.ReturnValue = result.Result;
            }
            else
            {
                throw new InvalidOperationException("Synchronous operations must not result in asynchronous actions.");
            }
        }

        public async ValueTask<T> InterceptInternalAsync(IInvocation invocation, bool isAsync, CancellationToken token)
        {
            var proceed = new Proceed<T>(invocation);

            if (!_options.Value.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            }

            var key = _keyProvider.GetKey(invocation.Method, invocation.Arguments);

            _logger.LogDebug("Checking for cached value for '{Key}'", key);

            var cached = await GetAsync(key, isAsync, token).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);

                return _serializer.GetValue(cached);
            }

            _logger.LogDebug("Did not find cached value for '{Key}'", key);

            var result = await proceed.InvokeAsync(isAsync).ConfigureAwait(false);
            var options = new DistributedCacheEntryOptions();

            foreach (var handler in _options.Value.CacheHandlers)
            {
                handler.ConfigureEntryOptions(typeof(T), invocation.Method, result, options);
            }

            var (bytes, finalResult) = _serializer.GetBytes(result);
            await SetAsync(key, bytes, options, isAsync, token).ConfigureAwait(false);

            _logger.LogDebug("Cached result for '{Key}'", key);

            return finalResult;
        }

        private async ValueTask SetAsync(string key, byte[] bytes, DistributedCacheEntryOptions options, bool isAsync, CancellationToken token)
        {
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
