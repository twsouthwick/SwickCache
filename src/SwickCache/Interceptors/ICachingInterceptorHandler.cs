using Castle.DynamicProxy;
using System.Threading;

namespace Swick.Cache
{
    internal interface ICachingInterceptorHandler
    {
        void Intercept(IInvocation invocation, bool isAsync, CancellationToken token);

        void Invalidate(IInvocation invocation, bool isAsync);
    }
}
