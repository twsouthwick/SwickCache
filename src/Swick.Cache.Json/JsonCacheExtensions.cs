using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Swick.Cache
{
    public static class JsonCacheExtensions
    {
        public static CacheBuilder AddJsonSerializer(this CacheBuilder builder)
        {
            builder.Services.TryAddSingleton(typeof(ICacheSerializer<>), typeof(JsonCacheSerializer<>));

            return builder;
        }
    }
}
