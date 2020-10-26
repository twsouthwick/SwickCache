using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CachingProxyGenerationHook : IProxyGenerationHook
    {
        private readonly ILogger<CachingProxyGenerationHook> _logger;
        private readonly IOptions<CachingOptions> _options;

        public CachingProxyGenerationHook(ILogger<CachingProxyGenerationHook> logger, IOptions<CachingOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            foreach (var shouldCache in _options.Value.ShouldCache)
            {
                if (shouldCache(type, methodInfo))
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
            => _options.GetHashCode();

        public override bool Equals(object obj)
            => obj is CachingProxyGenerationHook other && other._options == _options;
    }
}
