using AutofacContrib.NSubstitute;
using AutoFixture;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
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
        public void CachingManagerResolves()
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
        public async Task SimpleAsyncMethodIsCached()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();

            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test =>
                    {
                        test.Add(t => t.ReturnObjectAsync());
                    });
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

        [Fact]
        public async Task AsyncMethodWithCancellationIsCached()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test =>
                    {
                        test.Add(t => t.ReturnObjectAsyncWithCancellation(default));
                    });
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<IDistributedCache>(cache => cache.GetAsync(key, token).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ReturnObjectAsyncWithCancellation(token).Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = await cached.ReturnObjectAsyncWithCancellation(token);

            // Assert
            Assert.Equal(expected, result);

            await mock.Resolve<IDistributedCache>().Received(1).SetAsync(key, value, Arg.Any<DistributedCacheEntryOptions>(), token);
        }

        [Fact]
        public async Task ValueTaskMethodIsCached()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();

            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test =>
                    {
                        test.Add(t => t.ValueTaskObjectAsync());
                    });
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<IDistributedCache>(cache => cache.GetAsync(key).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ValueTaskObjectAsync().Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = await cached.ValueTaskObjectAsync();

            // Assert
            Assert.Equal(expected, result);

            await mock.Resolve<IDistributedCache>().Received(1).SetAsync(key, value, Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValueTaskCancellationIsCached()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test =>
                    {
                        test.Add(t => t.ValueTaskObjectWithCancellationAsync(default));
                    });
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<IDistributedCache>(cache => cache.GetAsync(key, token).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ValueTaskObjectWithCancellationAsync(token).Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = await cached.ValueTaskObjectWithCancellationAsync(token);

            // Assert
            Assert.Equal(expected, result);

            await mock.Resolve<IDistributedCache>().Received(1).SetAsync(key, value, Arg.Any<DistributedCacheEntryOptions>(), token);
        }

        [Fact]
        public void SimpleSyncMethodIsCached()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();
            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test =>
                    {
                        test.Add(t => t.ReturnObject());
                    });
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<IDistributedCache>(cache => cache.Get(key).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ReturnObject().Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = cached.ReturnObject();

            // Assert
            Assert.Equal(expected, result);

            mock.Resolve<IDistributedCache>().Received(1).Set(key, value, Arg.Any<DistributedCacheEntryOptions>());
        }

        [Fact]
        public void CustomCache()
        {
            // Arrange
            var expected = _fixture.Create<string>();
            var key = _fixture.Create<string>();
            var value = _fixture.CreateMany<byte>().ToArray();
            using var mock = AutoSubstitute.Configure()
                .InjectProperties()
                .AddCaching(c =>
                {
                    c.CacheType<ITest>(test => test
                        .WithCache<ICustomCache>()
                        .Add(t => t.ReturnObject()));
                })
                .MakeUnregisteredTypesPerLifetime()
                .ConfigureService<ICustomCache>(cache => cache.Get(key).Returns((byte[])null))
                .ConfigureService<ITest>(t => t.ReturnObject().Returns(expected))
                .SubstituteFor<ICacheKeyProvider>()
                    .ConfigureSubstitute(t => t.GetKey(Arg.Any<MethodInfo>(), Arg.Any<object[]>()).Returns(key))
                .ConfigureService<ICacheSerializer<object>>(t => t.GetBytes(expected).Returns((value, expected)))
                .Build();

            var cached = mock.Resolve<ICachingManager>().CreateCachedProxy(mock.Resolve<ITest>());

            // Act
            var result = cached.ReturnObject();

            // Assert
            Assert.Equal(expected, result);

            mock.Resolve<ICustomCache>().Received(1).Set(key, value, Arg.Any<DistributedCacheEntryOptions>());
        }

        public interface ICustomCache : IDistributedCache
        {
        }

        public interface ITest
        {
            Task<object> ReturnObjectAsync();

            Task<object> ReturnObjectAsyncWithCancellation(CancellationToken token);

            ValueTask<object> ValueTaskObjectWithCancellationAsync(CancellationToken token);

            ValueTask<object> ValueTaskObjectAsync();

            object ReturnObject();
        }
    }
}
