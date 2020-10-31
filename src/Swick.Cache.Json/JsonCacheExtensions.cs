using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Swick.Cache
{
    public static class JsonCacheExtensions
    {
        public static CacheBuilder AddJsonSerializer(this CacheBuilder builder, Action<JsonCacheOptions> configure = null)
        {
            var optionBuilder = builder.Services.AddOptions<JsonCacheOptions>();

            if (configure != null)
            {
                optionBuilder.Configure(configure);
            }

            builder.Services.TryAddSingleton<ICacheSerializer, JsonCacheSerializer>();

            return builder;
        }
    }
}
