using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CachingProxyGenerationHook : IProxyGenerationHook
    {
        private readonly CachingOptions _options;

        public CachingProxyGenerationHook(CachingOptions options)
        {
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
            foreach (var handler in _options.CacheHandlers)
            {
                if (handler.ShouldCache(type, methodInfo))
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
