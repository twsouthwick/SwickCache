using System.Text.Json;

namespace Swick.Cache
{
    internal class JsonCacheSerializer : ICacheSerializer
    {
        public byte[] GetBytes<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj);
        }

        public TResult GetValue<TResult>(byte[] data)
        {
            return JsonSerializer.Deserialize<TResult>(data);
        }
    }
}
