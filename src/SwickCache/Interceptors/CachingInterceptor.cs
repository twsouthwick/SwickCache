using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading;

namespace Swick.Cache
{
    internal sealed class CachingInterceptor : AsyncBaseInterceptor
    {
        public CachingInterceptor(IServiceProvider services)
            : base(services)
        {
        }

        protected override Action<IInvocation> CreateAction(MethodType methodType, ICachingInterceptorHandler handler, MethodInfo method, Type type)
        {
            if (methodType != MethodType.Synchronous && TryGetCancellationIndex(method, out var index))
            {
                return invocation => handler.Intercept(invocation, methodType, (CancellationToken)invocation.Arguments[index]);
            }
            else
            {
                return invocation => handler.Intercept(invocation, methodType, default);
            }
        }

        private static bool TryGetCancellationIndex(MethodInfo method, out int index)
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
    }
}
