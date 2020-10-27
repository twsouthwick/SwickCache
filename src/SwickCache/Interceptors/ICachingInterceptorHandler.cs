using Castle.DynamicProxy;
using System.Threading;

namespace Swick.Cache
{
    internal interface ICachingInterceptorHandler
    {
        void Intercept(IInvocation invocation, MethodType methodType, CancellationToken token);

        void Invalidate(IInvocation invocation, MethodType methodType);
    }
}
