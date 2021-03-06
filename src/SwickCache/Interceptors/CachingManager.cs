using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CachingManager : ICachingManager, IProxyGenerationHook
    {
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private readonly ILogger<CachingManager> _logger;
        private readonly CachingInterceptor _cachingInterceptor;
        private readonly CacheInvalidatorInterceptor _invalidatorInterceptor;
        private readonly ProxyGenerator _generator;
        private readonly ProxyGenerationOptions _options;

        public CachingManager(
            IOptionsMonitor<CachingOptions> options,
            ILogger<CachingManager> logger,
            IServiceProvider services)
        {
            _cachingOptions = options;
            _logger = logger;
            _cachingInterceptor = new CachingInterceptor(services);
            _invalidatorInterceptor = new CacheInvalidatorInterceptor(services);
            _generator = new ProxyGenerator();
            _options = new ProxyGenerationOptions(this);
        }

        public T CreateCachedProxy<T>(T target)
            where T : class
        {
            if (!_cachingOptions.CurrentValue.UseProxies)
            {
                _logger.LogWarning("Proxy generation is off. In order to cache objects of type {Type}, the application might need to be restarted.", typeof(T));
                return target;
            }

            return _generator.CreateInterfaceProxyWithTargetInterface(target, _options, _cachingInterceptor);
        }

        public T CreateInvalidatorProxy<T>()
            where T : class
        {
            return _generator.CreateInterfaceProxyWithoutTarget<T>(_options, _invalidatorInterceptor);
        }

        void IProxyGenerationHook.MethodsInspected()
        {
        }

        void IProxyGenerationHook.NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        bool IProxyGenerationHook.ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            foreach (var handler in _cachingOptions.CurrentValue.InternalHandlers)
            {
                if (handler.ShouldCache(type, methodInfo))
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
         => _cachingOptions.GetHashCode();

        public override bool Equals(object obj)
            => obj is CachingManager other && other._cachingOptions == _cachingOptions;

    }
}
