using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Swick.Cache
{
    public static class CachingExtensions
    {
        public static CacheBuilder AddCachingManager(this IServiceCollection services)
        {
            services.AddOptions<CachingOptions>()
                .Configure(options =>
                {
                    options.IsEnabled = true;
                    options.UseProxies = true;
                });

            services.AddOptions<CachingInterceptorHandlerOptions>()
                .Configure<IDistributedCache>((options, cache) => options.DefaultCache = cache);

            services.TryAddSingleton<ICachingManager, CachingManager>();
            services.TryAddSingleton(typeof(CachingInterceptorHandler<>));
            services.TryAddTransient<ICacheKeyProvider, CacheKeyProvider>();

            return new CacheBuilder(services);
        }

        public static CacheBuilder AddAccessors(this CacheBuilder builder)
        {
            builder.Services.TryAddTransient(typeof(ICacheInvalidator<>), typeof(CacheInvalidator<>));
            builder.Services.TryAddTransient(typeof(ICached<>), typeof(ProxyCached<>));

            return builder;
        }
    }
}
