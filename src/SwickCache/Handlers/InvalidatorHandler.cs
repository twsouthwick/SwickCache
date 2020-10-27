using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    public sealed class InvalidatorHandler<T> : CacheHandler
    {
        private readonly Action<T, DistributedCacheEntryOptions> _config;

        public InvalidatorHandler(Action<T, DistributedCacheEntryOptions> config)
        {
            _config = config;
        }

        protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
            if (obj is T t)
            {
                _config(t, options);
            }
        }
    }
}
