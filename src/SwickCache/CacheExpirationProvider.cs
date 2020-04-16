using System;
using System.Reflection;

namespace Swick.Cache
{
    internal class CacheExpirationProvider
    {
        public DateTimeOffset? GetExpiration<T>(MethodInfo method, T obj)
        {
            if (obj is ICacheInvalidator invalidator)
            {
                return invalidator.Expiration;
            }
            else
            {
                var attribute = method.GetCustomAttribute<CachedAttribute>();

                if (!attribute.Duration.HasValue)
                {
                    return null;
                }

                return DateTimeOffset.Now.Add(attribute.Duration.Value);
            }
        }
    }
}
