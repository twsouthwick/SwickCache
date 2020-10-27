using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swick.Cache.Serialization;
using System.IO;

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

            services.TryAddSingleton<ICachingManager, CachingManager>();
            services.TryAddSingleton(typeof(CachingInterceptorHandler<>));
            services.TryAddTransient<ICacheKeyProvider, CacheKeyProvider>();
            services.TryAddSingleton<ICacheSerializer<byte[]>, ByteArraySerializer>();
            services.TryAddSingleton<ICacheSerializer<Stream>, StreamSerialzier>();

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
