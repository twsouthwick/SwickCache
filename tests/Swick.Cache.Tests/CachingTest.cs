using AutofacContrib.NSubstitute;
using Microsoft.Extensions;
using Microsoft.Extensions.Caching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

using Assert = Swick.Cache.Tests.AssertExtensions;
using Microsoft.Extensions.Caching.Distributed;
using AutoFixture;
using System.Reflection;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Swick.Cache.Tests
{
    public class CachingTest
    {
        private readonly Fixture _fixture;

        public CachingTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Sanity()
        {
            // Arrange
            using var mock = AutoSubstitute.Configure()
                .ConfigureOptions(options =>
                {
                    options.AutomaticallySkipMocksForProvidedValues = true;
                    options.TypesToSkipForMocking.Add(typeof(IOptionsMonitor<CachingOptions>));
                })
                .AddCaching()
                .Build();

            // Act
            var manager = mock.Resolve<ICachingManager>();

            // Assert
            Assert.IsNotNSubstituteMock(manager);
        }

        [Fact]
        public async Task Sanity2()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();
            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<ITest>(t => t.ReturnStringAsync().Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer>(t => t.GetBytes(expected).Returns(value))
                .AddCaching()
                .Build();
            var cache = mock.Resolve<IDistributedCache>();
            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = await cached.ReturnStringAsync();

            // Assert
            Assert.Equal(expected, result);

            await cache.Received(1).SetAsync(key, value, Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
        }

        public interface ITest
        {
            Task<string> ReturnStringAsync();

            Task<string> ReturnStringAsync1Arg();
        }
    }
}
