using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Swick.Cache
{
    public class CacheBuilder
    {
        public CacheBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public CacheBuilder ConfigureOptions(Action<OptionsBuilder<CachingOptions>> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(Services.AddOptions<CachingOptions>());

            return this;
        }

        public CacheBuilder Configure(Action<CachingOptions> action)
            => ConfigureOptions(b => b.Configure(action));

        public CacheBuilder CacheType<T>(Action<CacheTypeBuilder<T>> builder, Action<DistributedCacheEntryOptions> configureEntry = null)
        {
            var types = new CacheTypeBuilder<T>(Services, configureEntry);

            builder(types);

            return Configure(options =>
            {
                options.CacheHandlers.Add(types.ToCacheHandler());
            });
        }
    }
}
