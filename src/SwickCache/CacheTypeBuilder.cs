using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Swick.Cache.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swick.Cache
{
    public class CacheTypeBuilder<T>
    {
        private readonly HashSet<MethodInfo> _methods = new HashSet<MethodInfo>();
        private readonly IServiceCollection _services;
        private readonly Action<DistributedCacheEntryOptions> _entryConfigure;

        private Action<MethodInfo> _customCache;

        internal CacheTypeBuilder(
            IServiceCollection services,
            Action<DistributedCacheEntryOptions> entryConfigure)
        {
            _services = services;
            _entryConfigure = entryConfigure;
        }

        public CacheTypeBuilder<T> WithCache<TCache>()
            where TCache : class, IDistributedCache
        {
            void AddCacheType(MethodInfo method) => _services.AddOptions<CachingInterceptorHandlerOptions>()
                .Configure<TCache>((options, cache) =>
                {
                    options.AddCache(typeof(T), method, cache);
                });

            _customCache = AddCacheType;

            return this;
        }

        public CacheTypeBuilder<T> Add(string name)
        {
            var methods = typeof(T).GetMethods()
                .Where(m => string.Equals(name, m.Name, StringComparison.Ordinal));

            foreach (var method in methods)
            {
                _methods.Add(method);
                _customCache?.Invoke(method);
            }

            return this;
        }

        public CacheTypeBuilder<T> Add<TMethod>(Expression<Func<T, TMethod>> expression)
        {
            if (expression.Body is MethodCallExpression m)
            {
                _methods.Add(m.Method);
                _customCache?.Invoke(m.Method);
            }
            else
            {
                throw new InvalidOperationException("Expression must return a method");
            }

            return this;
        }

        internal CacheHandler ToCacheHandler() => new CacheTypeBuilderHandler(_methods, _entryConfigure);

        private class CacheTypeBuilderHandler : CacheHandler
        {
            private readonly HashSet<MethodInfo> _methods;
            private readonly Action<DistributedCacheEntryOptions> _entryConfigure;

            public CacheTypeBuilderHandler(HashSet<MethodInfo> methods, Action<DistributedCacheEntryOptions> entryConfigure)
            {
                _methods = methods;
                _entryConfigure = entryConfigure;
            }

            protected internal override bool ShouldCache(Type type, MethodInfo methodInfo)
            {
                if (type == typeof(T))
                {
                    return _methods.Contains(methodInfo);
                }

                return false;
            }

            protected internal override void ConfigureEntryOptions(Type type, MethodInfo method, object obj, DistributedCacheEntryOptions options)
            {
                if (_entryConfigure is null)
                {
                    return;
                }

                if (type == typeof(T) && _methods.Contains(method))
                {
                    _entryConfigure(options);
                }
            }
        }
    }
}
