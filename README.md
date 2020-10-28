Swick.Cache
===========

![NuGet](https://img.shields.io/nuget/v/Swick.Cache)

A simple tool to easily create caching proxies for types. To use, use the following steps:

1. Register the caching manager to your services:
1. Register methods you want to be proxied. This can be done declaratively, via attributes, or by implementing a custom `CacheHandler`.
1. Register an implementation of `IDistributedCache`
1. Wire up the cache proxy generator:
    - Register accessors so you can request `ICached<T>` or `ICacheInvalidator<T>` instances that will provide instances that will cache or invalidate cache entries respectively.
    - Use Autofac, Scrutor, or other libraries to decorate a type `T` by calling `ICachingManager` methods to create proxies.
1. Register any serializer instances you may need (must implement `ICacheSerializer<>`) which are specialized per method return type.

Customization
-------------

In order to hook into the proxy creation and injection, you can add custom `CacheHandler` instances to `CacheOptions.CacheHandlers`. There are some built in ones that can help with common tasks, such as setting default expiration, set up an attribute to identify methods for caching, etc.

Examples
-------

```csharp
  public interface ITest
  {
      Task<object> ReturnObjectAsync();
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddCachingManager()
      .Configure(options =>
      {
          options.CacheHandlers.Add(new DefaultExpirationCacheHandler(entry =>
          {
              entry.SlidingExpiration = TimeSpan.FromDays(5);
          }));
      })
      .CacheType<ITest>(test => test.Add(nameof(ITest.ReturnObjectAsync)));
      .AddAccessors();
  }

  public class UsageTest
  {
    public UsageTest(ICached<ITest> cached, ICacheInvalidator<ITest> invalidator)
    {
      cached.Instance.ReturnObjectAsync();
    }
  }
```

If you want to decorate services, use Autofac, Scrutor or some other system to help with that:


```csharp
  public interface ITest
  {
      Task<object> ReturnObjectAsync();
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddCachingManager()
      .Configure(options =>
      {
          options.CacheHandlers.Add(new DefaultExpirationCacheHandler(entry =>
          {
              entry.SlidingExpiration = TimeSpan.FromDays(5);
          }));
      })
      .CacheType<ITest>(test => test.Add(nameof(ITest.ReturnObjectAsync)));
      .AddAccessors();

    services.Decorate<ITest>((other, ctx) => ctx.GetRequiredService<ICachingManager>().CreateCachedProxy(other));
 
  }

  public class UsageTest
  {
    public UsageTest(ITest cached)
    {
      cached.ReturnObjectAsync();
    }
  }
```

You can register an attribute to define the methods to cache as well as identify the expiration policy:

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class CachedAttribute : Attribute
{
    private readonly int _days;

    public CachedAttribute(int days)
    {
        _days = days;
    }

    public TimeSpan SlidingExpiration => TimeSpan.FromDays(_days);
}

public interface ITest
{
    [Cached(5)]
    Task<object> ReturnObjectAsync();
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddCachingManager()
      .Configure(options =>
      {
          options.CacheHandlers.Add(new AttributeCacheHandler<CachedAttribute>((a, entry) =>
          {
              entry.SlidingExpiration = a.SlidingExpiration;
          }));
      });

    services.Decorate<ITest>((other, ctx) => ctx.GetRequiredService<ICachingManager>().CreateCachedProxy(other));
}

public class UsageTest
{
    public UsageTest(ITest cached)
    {
        cached.ReturnObjectAsync();
    }
}
```