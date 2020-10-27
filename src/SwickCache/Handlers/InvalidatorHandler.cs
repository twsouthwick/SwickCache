using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    internal class InvalidatorHandler : CacheHandler
    {
        public static CacheHandler Instance { get; } = new InvalidatorHandler();

        private InvalidatorHandler()
        {
        }

        protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
            if (obj is ICacheInvalidator invalidator)
            {
                options.AbsoluteExpiration = invalidator.Expiration;
            }
        }
    }
}
