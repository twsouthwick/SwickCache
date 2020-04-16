using Swick.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Swick.Cache
{
    public class CacheBuilder
    {
        public CacheBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public OptionsBuilder<CachingOptions> Configure() => Services.AddOptions<CachingOptions>();
    }
}
