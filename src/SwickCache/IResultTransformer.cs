using System.Threading;
using System.Threading.Tasks;

namespace Swick.Cache
{
    public interface IResultTransformer<T>
    {
        ValueTask<T> TransformAsync(T input, CancellationToken token);

        ValueTask<T> ResetAsync(T input, CancellationToken token);
    }
}
