using Castle.DynamicProxy;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal readonly struct Proceed<TResult>
    {
        private readonly IInvocation _invocation;
        private readonly IInvocationProceedInfo _proceed;

        public Proceed(IInvocation invocation)
        {
            _invocation = invocation;
            _proceed = invocation.CaptureProceedInfo();
        }

        public async ValueTask<TResult> InvokeAsync(bool isAsync)
        {
            _proceed.Invoke();

            if (isAsync)
            {
                return await (Task<TResult>)_invocation.ReturnValue;
            }
            else
            {
                return (TResult)_invocation.ReturnValue;
            }
        }
    }
}
