using Microsoft.Extensions.DependencyInjection;
using System;

namespace Swick.Cache.Tests
{

    public abstract class CachingTestBase : IDisposable
    {
        private readonly ServiceProvider _services;

        public CachingTestBase()
        {
            var services = new ServiceCollection();

            CustomizeCaching(services.AddCaching());

            _services = services.BuildServiceProvider();
        }

        public void Dispose() => _services.Dispose();

        protected virtual void CustomizeCaching(CacheBuilder builder)
        {
        }
    }
}
