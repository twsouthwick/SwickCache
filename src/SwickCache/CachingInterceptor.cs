using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swick.Cache.Interceptors;
using System;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CachingInterceptor : MyAsyncBase, ICacheEntryInvalidator
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

        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo info, Func<IInvocation, IInvocationProceedInfo, Task> proceed) => proceed(invocation, info);

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo info, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> _)
        {
            var proceed = new Proceed<TResult>(invocation, info);

            if (!_options.CurrentValue.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed.InvokeAsync().ConfigureAwait(false);
            }

            var key = GetCacheKey(invocation);

            var cached = await GetAsync(key).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);

                return GetValue(cached);
            }

            var result = await proceed.InvokeAsync().ConfigureAwait(false);
            var expiration = _expirationProvider.GetExpiration(invocation.Method, result);

            if (expiration.HasValue)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration.Value,
                };

                await _cache.SetAsync(key, _serializer.GetBytes(result), options).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("No expiration is defined for {Invocation}", invocation);
                await _cache.SetAsync(key, GetBytes(result)).ConfigureAwait(false);
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

        private async Task<byte[]> GetAsync(string key)
        {
            try
            {
                return await _cache.GetAsync(key).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while accessing cached value.");
                return null;
            }
        }
    }
}
