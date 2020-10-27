using Microsoft.Extensions.Caching.Distributed;
using Swick.Cache.Handlers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swick.Cache
{
    public class CacheTypeBuilder<T> : CacheHandler
    {
        private readonly HashSet<MethodInfo> _methods = new HashSet<MethodInfo>();
        private readonly Action<DistributedCacheEntryOptions> _entryConfigure;

        public CacheTypeBuilder(Action<DistributedCacheEntryOptions> entryConfigure)
        {
            _entryConfigure = entryConfigure;
        }

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
