using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CacheInvalidatorInterceptor : CachingInterceptor
    {
        private readonly ICacheKeyProvider _keyProvider;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheInvalidatorInterceptor> _logger;

        public CacheInvalidatorInterceptor(
            ICacheKeyProvider keyProvider,
            IDistributedCache cache,
            ILogger<CacheInvalidatorInterceptor> logger,
            IServiceProvider services)
            : base(services)
        {
            _keyProvider = keyProvider;
            _cache = cache;
            _logger = logger;
        }

        protected override Action<IInvocation> CreateHandler(MethodInfo method)
        {
            var (methodType, returnType) = GetMethodType(method.ReturnType);

            switch (methodType)
            {
                case MethodType.SynchronousVoid:
                case MethodType.AsyncAction:
                    return invocation => invocation.Proceed();
                case MethodType.AsyncFunction:
                    var handler = (ICachingInterceptorHandler)_services.GetRequiredService(typeof(CachingInterceptorHandler<>).MakeGenericType(returnType));
                    return invocation => handler.Invalidate(invocation, isAsync: true);
                case MethodType.Synchronous:
                default:
                    var handler2 = (ICachingInterceptorHandler)_services.GetRequiredService(typeof(CachingInterceptorHandler<>).MakeGenericType(returnType));
                    return invocation => handler2.Invalidate(invocation, isAsync: false);
            }
        }
    }
}
