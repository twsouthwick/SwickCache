using System.Threading;
using System.Threading.Tasks;

namespace Swick.Cache
{
    public interface ICacheSerializer<T>
    {
        /// <summary>
        /// Gets whether <typeparamref name="T"/> can be used after serialization. if false, the deserializer will be called on its result.
        /// </summary>
        bool IsImmutable(T input);

        byte[] GetBytes(T obj);

        T GetValue(byte[] data);
    }
}
