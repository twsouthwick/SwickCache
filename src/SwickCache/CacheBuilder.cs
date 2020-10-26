﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace Swick.Cache
{
    public class CacheBuilder
    {
        public CacheBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public CacheBuilder Configure(Action<OptionsBuilder<CachingOptions>> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(Services.AddOptions<CachingOptions>());

            return this;
        }

        public CacheBuilder CacheType<T>(Action<CacheTypeBuilder<T>> builder)
        {
            Configure(b =>
            {
                var types = new CacheTypeBuilder<T>();

                builder(types);

                b.Configure(options =>
                {
                    options.ShouldCache.Add(types.ShouldAdd);
                });
            });

            return this;
        }

        public class CacheTypeBuilder<T>
        {
            private readonly HashSet<MethodInfo> _methods = new HashSet<MethodInfo>();

            public CacheTypeBuilder<T> Add<TMethod>(Expression<Func<T, TMethod>> expression)
            {
                if (expression.Body is MethodCallExpression m)
                {
                    _methods.Add(m.Method);
                }
                else
                {
                    throw new InvalidOperationException("Expression must return a method");
                }

                return this;
            }

            internal bool ShouldAdd(Type type, MethodInfo method)
            {
                if (type == typeof(T))
                {
                    return _methods.Contains(method);
                }

                return false;
            }
        }
    }
}
