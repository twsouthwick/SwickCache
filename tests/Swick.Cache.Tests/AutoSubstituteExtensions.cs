using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacContrib.NSubstitute;
using AutofacContrib.NSubstitute.MockHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Swick.Cache.Tests
{
    public static class AutoSubstituteExtensions
    {
        public static AutoSubstituteBuilder ConfigureService<T>(this AutoSubstituteBuilder builder, Action<T> configure)
            => builder.ConfigureBuilder(b => b.RegisterBuildCallback(ctx =>
            {
                var t = ctx.Resolve<T>();
                configure(t);
            }));

        public static AutoSubstituteBuilder AddInMemoryCache(this AutoSubstituteBuilder builder)
            => builder.AddServices(s => s.AddDistributedMemoryCache(options => { }));

        public static AutoSubstituteBuilder AddCaching(this AutoSubstituteBuilder builder)
            => builder.AddCaching(_ => { });

        public static AutoSubstituteBuilder AddCaching(this AutoSubstituteBuilder builder, Action<CacheBuilder> cacheBuilder)
        {
            builder.ConfigureOptions(options =>
            {
                options.MockHandlers.Add(SkipValidateOptionsMockHandler.Instance);
            });

            return builder.AddServices(b =>
            {
                cacheBuilder(b.AddCaching());
            });
        }

        public static AutoSubstituteBuilder AddServices(this AutoSubstituteBuilder builder, Action<IServiceCollection> servicesBuilder)
        {
            var services = new ServiceCollection();

            servicesBuilder(services);

            return builder.ConfigureBuilder(b =>
            {
                b.Populate(services);
            });
        }

        private class SkipValidateOptionsMockHandler : MockHandler
        {
            public static MockHandler Instance { get; } = new SkipValidateOptionsMockHandler();

            protected override void OnMockCreating(MockCreatingContext context)
            {
                if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(IValidateOptions<>))
                {
                    context.DoNotCreate();
                }
            }
        }


    }
}
