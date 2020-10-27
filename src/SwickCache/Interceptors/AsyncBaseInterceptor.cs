using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal abstract class AsyncBaseInterceptor : IInterceptor
    {
        private static readonly Action<IInvocation> _proceed = invocation => invocation.Proceed();

        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> _handlers = new ConcurrentDictionary<MethodInfo, Action<IInvocation>>();

        public AsyncBaseInterceptor(IServiceProvider services)
        {
            _services = services;
        }

        public void Intercept(IInvocation invocation)
        {
            var handler = _handlers.GetOrAdd(invocation.Method, CreateHandler);
            handler(invocation);
        }

        protected abstract Action<IInvocation> CreateAction(MethodType methodType, ICachingInterceptorHandler handler, MethodInfo method, Type type);

        private Action<IInvocation> CreateHandler(MethodInfo method)
            => GetMethodType(method.ReturnType) switch
            {
                (MethodType.Void, _) => _proceed,
                (var methodType, var returnType) => CreateAction(methodType, CreateHandler(returnType), method, returnType),
            };

        private ICachingInterceptorHandler CreateHandler(Type returnType)
            => (ICachingInterceptorHandler)_services.GetRequiredService(typeof(CachingInterceptorHandler<>).MakeGenericType(returnType));

        private static (MethodType, Type) GetMethodType(Type returnType)
        {
            if (returnType == typeof(void))
            {
                return (MethodType.Void, typeof(void));
            }

            if (returnType.IsGenericType)
            {
                var genericType = returnType.GetGenericTypeDefinition();
                var arg = returnType.GetGenericArguments()[0];

                if (genericType == typeof(Task<>))
                {
                    return (MethodType.Task, arg);
                }
                else if (genericType == typeof(ValueTask<>))
                {
                    return (MethodType.ValueTask, arg);
                }
            }

            return (MethodType.Synchronous, returnType);
        }
    }
}
