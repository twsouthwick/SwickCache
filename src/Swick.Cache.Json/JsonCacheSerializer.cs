using System.Text.Json;

namespace Swick.Cache
{
    public class JsonCacheSerializer<T> : ICacheSerializer<T>
    {
        public T GetValue(byte[] data)
            => JsonSerializer.Deserialize<T>(data);

        public (byte[] bytes, T result) GetBytes(T obj)
            => (JsonSerializer.SerializeToUtf8Bytes<T>(obj), obj);
    }
}
