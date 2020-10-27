using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CacheInvalidatorInterceptor : AsyncInterceptorBase
    {
        private readonly ICacheKeyProvider _keyProvider;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheInvalidatorInterceptor> _logger;

        public CacheInvalidatorInterceptor(
            ICacheKeyProvider keyProvider,
            IDistributedCache cache,
            ILogger<CacheInvalidatorInterceptor> logger)
        {
            _keyProvider = keyProvider;
            _cache = cache;
            _logger = logger;
        }

        protected override Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed) => proceed(invocation);

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            var key = _keyProvider.GetKey(invocation.Method, invocation.Arguments);

            _logger.LogTrace("Removing cached value for {Key}", key);

            await _cache.RemoveAsync(key);

            _logger.LogTrace("Removed cached value for {Key}", key);

            return default;
        }
    }
}
