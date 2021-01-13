using Castle.DynamicProxy;
using Swick.Cache.Resources;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal readonly struct Proceed<TResult>
    {
        private readonly MethodType _methodType;
        private readonly IInvocation _invocation;
        private readonly IInvocationProceedInfo _proceed;

        public Proceed(IInvocation invocation, MethodType methodType)
        {
            _methodType = methodType;
            _invocation = invocation;
            _proceed = invocation.CaptureProceedInfo();
        }

        public async ValueTask<TResult> InvokeAsync()
        {
            _proceed.Invoke();

            if (_methodType == MethodType.Task)
            {
                if (_invocation.ReturnValue is Task<TResult> resultTask)
                {
                    return await resultTask;
                }

                throw new CacheException(LocalizedStrings.TaskReturnedNull);
            }
            else if (_methodType == MethodType.ValueTask)
            {
                return await (ValueTask<TResult>)_invocation.ReturnValue;
            }
            else
            {
                return (TResult)_invocation.ReturnValue;
            }
        }
    }
}
