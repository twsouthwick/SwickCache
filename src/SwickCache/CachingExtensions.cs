using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
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

        public static CacheBuilder CacheAttribute(this CacheBuilder builder)
            => builder.Configure(options =>
            {
                options.CacheHandlers.Add(new CachedAttributeHandler());
            });

        private class CachedAttributeHandler : CacheHandler
        {
            private readonly ConcurrentDictionary<MethodInfo, DateTimeOffset?> _expirations = new ConcurrentDictionary<MethodInfo, DateTimeOffset?>();

            protected internal override bool ShouldCache(Type type, MethodInfo methodInfo)
            {
                return methodInfo.GetCustomAttribute<CachedAttribute>() != null;
            }

            protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
            {
                var expiration = _expirations.GetOrAdd(method, m =>
                {
                    var attribute = m.GetCustomAttribute<CachedAttribute>();

                    if (attribute is null)
                    {
                        return null;
                    }

                    if (!attribute.Duration.HasValue)
                    {
                        return null;
                    }

                    return DateTimeOffset.Now.Add(attribute.Duration.Value);
                });

                if (expiration.HasValue)
                {
                    options.AbsoluteExpiration = expiration.Value;
                }
            }
        }
    }
}
