using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System;
using System.Reflection;

namespace Swick.Cache.Handlers
{
    public abstract class CacheHandler
    {
        /// <summary>
        /// Identifies types that the data is drained during serialization. This will require deserialization
        /// to occur before returning a value that has been serialized. Examples of this are <see cref="Stream"/>..
        /// </summary>
        /// <param name="obj">Type being checked.</param>
        /// <returns><c>true</c> if data has been drained during serialization.</returns>
        protected internal virtual bool IsDataDrained(object obj) => false;

        protected internal virtual bool ShouldCache(Type type, MethodInfo methodInfo)
            => false;

        protected internal virtual void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
        {
        }

        protected internal virtual bool ShouldCache(object obj)
            => true;
    }
}
