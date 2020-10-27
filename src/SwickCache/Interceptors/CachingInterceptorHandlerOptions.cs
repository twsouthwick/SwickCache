using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swick.Cache
{
    internal class CachingInterceptorHandlerOptions
    {
        private readonly Dictionary<(Type, MethodInfo), IDistributedCache> _caches = new Dictionary<(Type, MethodInfo), IDistributedCache>();

        public IDistributedCache DefaultCache { get; set; }

        public void AddCache(Type type, MethodInfo method, IDistributedCache cache)
        {
            _caches.Add((type, method), cache);
        }

        public IDistributedCache GetCache(Type type, MethodInfo method)
        {
            if (_caches.TryGetValue((type, method), out var cache))
            {
                return cache;
            }

            return DefaultCache;
        }
    }
}
