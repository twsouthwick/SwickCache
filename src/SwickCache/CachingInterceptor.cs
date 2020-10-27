using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal class CachingInterceptor : IInterceptor
    {
        protected readonly IServiceProvider _services;

        private readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> _handlers = new ConcurrentDictionary<MethodInfo, Action<IInvocation>>();

        public CachingInterceptor(IServiceProvider services)
        {
            _services = services;
        }

        private enum MethodType
        {
            SynchronousVoid,
            Synchronous,
            AsyncAction,
            AsyncFunction,
        }

        public void Intercept(IInvocation invocation)
        {
            var handler = _handlers.GetOrAdd(invocation.Method, CreateHandler);
            handler(invocation);
        }

        protected virtual Action<IInvocation> CreateAsyncHandler(ICachingInterceptorHandler handler, MethodInfo method, Type type)
        {
            if (TryGetCancellationIndex(method, out var index))
            {
                return invocation => handler.Intercept(invocation, true, (CancellationToken)invocation.Arguments[index]);
            }
            else
            {
                return invocation => handler.Intercept(invocation, true, default);
            }
        }

        protected virtual Action<IInvocation> CreateSyncHandler(ICachingInterceptorHandler handler, MethodInfo method, Type type)
            => invocation => handler.Intercept(invocation, isAsync: false, default);

        private Action<IInvocation> CreateHandler(MethodInfo method)
        {
            var (methodType, returnType) = GetMethodType(method.ReturnType);

            if (methodType == MethodType.SynchronousVoid || methodType == MethodType.AsyncAction)
            {
                return invocation => invocation.Proceed();
            }

            var handler = (ICachingInterceptorHandler)_services.GetRequiredService(typeof(CachingInterceptorHandler<>).MakeGenericType(returnType));

            if (methodType == MethodType.AsyncFunction)
            {
                return CreateAsyncHandler(handler, method, returnType);
            }

            return CreateSyncHandler(handler, method, returnType);
        }

        private bool TryGetCancellationIndex(MethodInfo method, out int index)
        {
            index = 0;
            foreach (var arg in method.GetParameters())
            {
                if (arg.ParameterType == typeof(CancellationToken))
                {
                    return true;
                }

                index++;
            }
            return false;
        }

        private static (MethodType, Type) GetMethodType(Type returnType)
        {
            if (returnType == typeof(void))
            {
                return (MethodType.SynchronousVoid, typeof(void));
            }

            if (!typeof(Task).IsAssignableFrom(returnType))
            {
                return (MethodType.Synchronous, returnType);
            }

            // The return type is a task of some sort, so assume it's asynchronous
            var isGeneric = returnType.GetTypeInfo().IsGenericType;

            if (isGeneric)
            {
                return (MethodType.AsyncFunction, returnType.GetGenericArguments()[0]);
            }

            return (MethodType.AsyncAction, typeof(Task));
        }
    }
}
