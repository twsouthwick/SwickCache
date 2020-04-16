using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Swick.Cache
{
    public static class JsonCacheExtensions
    {
        public static CacheBuilder AddJsonSerializer(this CacheBuilder builder)
        {
            builder.Services.TryAddSingleton<ICacheSerializer, JsonCacheSerializer>();

            return builder;
        }
    }
}
