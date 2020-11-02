using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    public abstract class CacheHandler<T> : CacheHandler
    {
        protected internal virtual bool IsDataDrained(T obj)
            => false;

        protected internal virtual bool ShouldCache(T obj)
            => true;
    }

    public abstract class CacheHandler
    {
        protected internal virtual bool ShouldCache(Type type, MethodInfo methodInfo)
            => false;

        protected internal virtual void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
        }
    }
}
