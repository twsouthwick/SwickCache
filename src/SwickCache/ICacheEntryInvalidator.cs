using Castle.DynamicProxy;
using System.Threading.Tasks;

namespace Swick.Cache
{
    internal interface ICacheEntryInvalidator
    {
        Task InvalidateAsync(IInvocation invocation);
    }
}
