using Castle.DynamicProxy;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CacheInvalidatorInterceptor : AsyncBaseInterceptor
    {
        public CacheInvalidatorInterceptor(IServiceProvider services)
            : base(services)
        {
        }

        protected override Action<IInvocation> CreateAction(MethodType methodType, ICachingInterceptorHandler handler, MethodInfo method, Type type)
            => invocation => handler.Invalidate(invocation, methodType);
    }
}
