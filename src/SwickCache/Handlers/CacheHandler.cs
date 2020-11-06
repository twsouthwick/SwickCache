using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    public abstract class CacheHandler
    {
        protected internal virtual bool ShouldCache(Type type, MethodInfo methodInfo)
            => false;

        protected internal virtual void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
        }

        protected internal virtual bool ShouldCache(object obj)
            => true;
    }
}
