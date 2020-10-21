using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacContrib.NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Swick.Cache.Tests
{
    public static class AutoSubstituteExtensions
    {
        public static AutoSubstituteBuilder ConfigureService<T>(this AutoSubstituteBuilder builder, Action<T> configure)
            => builder.ConfigureBuilder(b => b.RegisterBuildCallback(ctx => configure(ctx.Resolve<T>())));

        public static AutoSubstituteBuilder AddInMemoryCache(this AutoSubstituteBuilder builder)
            => builder.AddServices(s => s.AddDistributedMemoryCache(options => { }));

        public static AutoSubstituteBuilder AddCaching(this AutoSubstituteBuilder builder)
            => builder.AddServices(b =>
            {
                b.AddOptions();
                b.AddCaching();
            });

        public static AutoSubstituteBuilder AddCaching(this AutoSubstituteBuilder builder, Action<CacheBuilder> cacheBuilder)
            => builder.AddServices(b => cacheBuilder(b.AddCaching()));

        public static AutoSubstituteBuilder AddServices(this AutoSubstituteBuilder builder, Action<IServiceCollection> servicesBuilder)
        {
            var services = new ServiceCollection();

            servicesBuilder(services);

            return builder.ConfigureBuilder(b =>
            {
                b.Populate(services);
            });
        }
    }
}
