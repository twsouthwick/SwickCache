using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Swick.Cache
{
    internal class CacheExpirationProvider
    {
        private readonly ConcurrentDictionary<MethodInfo, DateTimeOffset?> _expirations = new ConcurrentDictionary<MethodInfo, DateTimeOffset?>();

        public DateTimeOffset? GetExpiration<T>(MethodInfo method, T obj)
        {
            if (obj is ICacheInvalidator invalidator)
            {
                return invalidator.Expiration;
            }
            else
            {
                return _expirations.GetOrAdd(method, m =>
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
            }
        }
    }
}
