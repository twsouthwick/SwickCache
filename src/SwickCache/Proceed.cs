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

        public Task<TResult> InvokeAsync()
        {
            _proceed.Invoke();
            return (Task<TResult>)_invocation.ReturnValue;
        }
    }
}
