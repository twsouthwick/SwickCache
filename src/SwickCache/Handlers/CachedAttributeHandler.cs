using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    internal class CachedAttributeHandler<TAttribute> : CacheHandler
        where TAttribute : Attribute
    {
        private readonly Action<TAttribute, DistributedCacheEntryOptions> _config;
        private readonly ConcurrentDictionary<MethodInfo, TAttribute> _attributes;

        public CachedAttributeHandler(Action<TAttribute, DistributedCacheEntryOptions> config)
        {
            _config = config;
            _attributes = new ConcurrentDictionary<MethodInfo, TAttribute>();
        }

        protected internal override bool ShouldCache(Type type, MethodInfo methodInfo)
             => GetAttribute(methodInfo) != null;

        protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
            var attribute = GetAttribute(method);

            if (attribute is null)
            {
                return;
            }

            _config(attribute, options);
        }

        private TAttribute GetAttribute(MethodInfo method)
            => _attributes.GetOrAdd(method, m => m.GetCustomAttribute<TAttribute>());
    }
}
