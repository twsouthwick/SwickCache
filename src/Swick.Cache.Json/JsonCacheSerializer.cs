using System.Text.Json;

namespace Swick.Cache
{
    public class JsonCacheSerializer<T> : ICacheSerializer<T>
    {
        private readonly JsonCacheOptions _options;

        public JsonCacheSerializer(JsonCacheOptions options)
        {
            _options = options;
        }

        public bool IsImmutable(T input) => true;

        public T GetValue(byte[] data)
            => JsonSerializer.Deserialize<T>(data, _options.JsonOptions);

        public byte[] GetBytes(T obj)
            => JsonSerializer.SerializeToUtf8Bytes<T>(obj, _options.JsonOptions);
    }
}
