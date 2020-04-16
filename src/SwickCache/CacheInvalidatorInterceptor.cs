using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CacheInvalidatorInterceptor : AsyncInterceptorBase
    {
        private readonly ICacheEntryInvalidator _invalidator;

        public CacheInvalidatorInterceptor(
            ICacheEntryInvalidator invalidator)
        {
            _invalidator = invalidator;
        }

        protected override Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed) => proceed(invocation);

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            await _invalidator.InvalidateAsync(invocation).ConfigureAwait(false);

            return default;
        }
    }
}
