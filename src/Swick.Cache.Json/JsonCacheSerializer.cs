using System.Text.Json;

namespace Swick.Cache
{
    public class JsonCacheSerializer : ICacheSerializer
    {
        public TResult GetValue<TResult>(byte[] data)
            => JsonSerializer.Deserialize<TResult>(data);

        public (byte[] bytes, T result) GetBytes<T>(T obj)
            => (JsonSerializer.SerializeToUtf8Bytes<T>(obj), obj);
    }
}
