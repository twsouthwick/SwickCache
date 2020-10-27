using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CacheInvalidatorInterceptor : CachingInterceptor
    {
        public CacheInvalidatorInterceptor(IServiceProvider services)
            : base(services)
        {
        }

        protected override Action<IInvocation> CreateSyncHandler(ICachingInterceptorHandler handler, MethodInfo method, Type type)
            => invocation => handler.Invalidate(invocation, isAsync: false);

        protected override Action<IInvocation> CreateAsyncHandler(ICachingInterceptorHandler handler, MethodInfo method, Type type)
            => invocation => handler.Invalidate(invocation, isAsync: true);
    }
}
