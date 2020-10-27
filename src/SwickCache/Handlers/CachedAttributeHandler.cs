using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    internal class CachedAttributeHandler : CacheHandler
    {
        private readonly ConcurrentDictionary<MethodInfo, DateTimeOffset?> _expirations = new ConcurrentDictionary<MethodInfo, DateTimeOffset?>();

        protected internal override bool ShouldCache(Type type, MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<CachedAttribute>() != null;
        }

        protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
            var expiration = _expirations.GetOrAdd(method, m =>
            {
                var attribute = m.GetCustomAttribute<CachedAttribute>();

                if (attribute is null)
                {
                    return null;
                }

                if (!attribute.Duration.HasValue)
                {
                    return null;
                }

                return DateTimeOffset.Now.Add(attribute.Duration.Value);
            });

            if (expiration.HasValue)
            {
                options.AbsoluteExpiration = expiration.Value;
            }
        }
    }
}
