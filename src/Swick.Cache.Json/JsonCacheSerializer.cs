using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Swick.Cache.Json
{
    public class JsonCacheSerializer : ICacheSerializer
    {
        private readonly JsonCacheOptions _options;

        public JsonCacheSerializer(IOptions<JsonCacheOptions> options)
        {
            _options = options.Value;
        }

        public T GetValue<T>(byte[] data)
            => JsonSerializer.Deserialize<T>(data, _options.JsonOptions);

        public byte[] GetBytes<T>(T obj)
            => JsonSerializer.SerializeToUtf8Bytes<T>(obj, _options.JsonOptions);
    }
}
