using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    internal class DefaultExpirationCacheHandler : CacheHandler
    {
        private readonly Action<DistributedCacheEntryOptions> _configure;

        public DefaultExpirationCacheHandler(Action<DistributedCacheEntryOptions> configure)
        {
            _configure = configure;
        }

        protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
            _configure(options);
        }
    }
}
