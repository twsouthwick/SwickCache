using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

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
                    options.ShouldCache.Add(HasCachedAttribute);
                });

            services.TryAddSingleton<ICachingManager, CachingManager>();
            services.TryAddSingleton<CachingInterceptor>();
            services.TryAddSingleton<CacheExpirationProvider>();
            services.TryAddSingleton<CachingProxyGenerationHook>();
            services.TryAddSingleton<ICacheEntryInvalidator>(ctx => ctx.GetRequiredService<CachingInterceptor>());
            services.TryAddSingleton<CacheInvalidatorInterceptor>();
            services.TryAddTransient(typeof(ICacheInvalidator<>), typeof(CacheInvalidator<>));
            services.TryAddTransient(typeof(ICached<>), typeof(ProxyCached<>));
            services.TryAddTransient<ICacheKeyProvider, CacheKeyProvider>();
            services.TryAddSingleton<ICacheSerializer, DefaultSerializer>();

            return new CacheBuilder(services);
        }

        private static bool HasCachedAttribute(Type _, MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<CachedAttribute>() != null;
        }
    }
}
