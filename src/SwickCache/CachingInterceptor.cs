using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CachingInterceptor : AsyncInterceptorBase, ICacheEntryInvalidator
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

        protected override Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed) => proceed(invocation);

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            if (!_options.CurrentValue.IsEnabled)
            {
                _logger.LogWarning("Caching has been turned off");

                return await proceed(invocation).ConfigureAwait(false);
            }

            var key = GetCacheKey(invocation);

            var cached = await _cache.GetAsync(key).ConfigureAwait(false);

            if (cached != null)
            {
                _logger.LogDebug("Using cached value for '{Key}'", key);
                return _serializer.GetValue<TResult>(cached);
            }

            var result = await proceed(invocation).ConfigureAwait(false);
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
                await _cache.SetAsync(key, _serializer.GetBytes(result)).ConfigureAwait(false);
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
    }
}
