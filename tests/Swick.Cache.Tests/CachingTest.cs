using AutofacContrib.NSubstitute;
using AutoFixture;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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
                .AddCaching(c =>
                {
                    c.CacheAttribute();
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<IDistributedCache>(cache => cache.GetAsync(key).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ReturnObjectAsync().Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = await cached.ReturnObjectAsync();

            // Assert
            Assert.Equal(expected, result);

            await mock.Resolve<IDistributedCache>().Received(1).SetAsync(key, value, Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
        }

        public interface ITest
        {
            [Cached]
            Task<object> ReturnObjectAsync();
        }
    }
}
