using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Swick.Cache
{
    public static class NewtonsoftCacheExtensions
    {
        public static CacheBuilder AddNewtonsoftSerializer(this CacheBuilder builder)
        {
            builder.Services.TryAddSingleton<ICacheSerializer, NewtonsoftCacheSerializer>();

            return builder;
        }
    }
}
