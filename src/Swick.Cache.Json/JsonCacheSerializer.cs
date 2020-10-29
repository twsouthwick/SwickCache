using System.Text.Json;

namespace Swick.Cache
{
    public class JsonCacheSerializer<T> : ICacheSerializer<T>
    {
        public bool IsImmutable(T input) => true;

        public T GetValue(byte[] data)
            => JsonSerializer.Deserialize<T>(data);

        public byte[] GetBytes(T obj)
            => JsonSerializer.SerializeToUtf8Bytes<T>(obj);
    }
}
