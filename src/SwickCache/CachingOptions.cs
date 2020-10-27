using Swick.Cache.Handlers;
using System.Collections.Generic;

namespace Swick.Cache
{
    public class CachingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether caching is turned on. Can be adjusted at runtime.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether proxies should be used. This requires a restart. If set to <c>false</c>, <see cref="IsEnabled" /> has no effect.
        /// </summary>
        public bool UseProxies { get; set; }

        /// <summary>
        /// Collection to identify what which methods should be cached.
        /// </summary>
        public IList<CacheHandler> CacheHandlers { get; } = new List<CacheHandler>();
    }
}
