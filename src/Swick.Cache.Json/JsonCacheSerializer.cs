using System.Text.Json;

namespace Swick.Cache
{
    public class JsonCacheSerializer : ICacheSerializer
    {
        public byte[] GetBytes<T>(T obj)
            => JsonSerializer.SerializeToUtf8Bytes<T>(obj);

        public TResult GetValue<TResult>(byte[] data)
            => JsonSerializer.Deserialize<TResult>(data);
    }
}
