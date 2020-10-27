using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Swick.Cache
{
    internal class CachingInterceptor : IAsyncInterceptor
    {
        private readonly IDistributedCache _cache;
        private readonly IOptionsMonitor<CachingOptions> _options;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly ILogger<CachingInterceptor> _logger;
        private readonly IServiceProvider _services;

        public CachingInterceptor(
            IDistributedCache cache,
            IOptionsMonitor<CachingOptions> options,
            ICacheKeyProvider keyProvider,
            ILogger<CachingInterceptor> logger,
            IServiceProvider services)
        {
            _cache = cache;
            _options = options;
            _keyProvider = keyProvider;
            _logger = logger;
            _services = services;
        }

        private static readonly MethodInfo HandleSyncMethodInfo = typeof(CachingInterceptor)
            .GetMethod(nameof(HandleSynchronous), BindingFlags.Static | BindingFlags.NonPublic);

        private delegate void SynchronousHandler(IServiceProvider interceptor, IInvocation invocation);

        private static readonly ConcurrentDictionary<Type, SynchronousHandler> _handlers = new ConcurrentDictionary<Type, SynchronousHandler>();

        public void InterceptSynchronous(IInvocation invocation)
        {
            if (invocation.Method.ReturnType != null)
            {
                var handler = _handlers.GetOrAdd(invocation.Method.ReturnType, t => (SynchronousHandler)HandleSyncMethodInfo.MakeGenericMethod(t).CreateDelegate(typeof(SynchronousHandler)));

                handler(_services, invocation);
            }
        }

        private static void HandleSynchronous<TResult>(IServiceProvider services, IInvocation invocation)
        {
            var interceptor = (ICachingInterceptor)services.GetService(typeof(CachingInterceptor<TResult>));
            interceptor.Intercept(invocation, isAsync: true, default);
        }

        public void InterceptAsynchronous(IInvocation invocation)
            => invocation.Proceed();

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var cancellationToken = invocation.Arguments.OfType<CancellationToken>().FirstOrDefault();

            var interceptor = (ICachingInterceptor)_services.GetService(typeof(CachingInterceptor<TResult>));
            interceptor.Intercept(invocation, isAsync: true, default);
        }
    }
}
