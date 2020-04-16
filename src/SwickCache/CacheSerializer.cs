using Newtonsoft.Json;
using System.IO;

namespace Swick.Cache
{
    internal class CacheSerializer : ICacheSerializer
    {
        private readonly JsonSerializer _serializer;

        public CacheSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] GetBytes(object obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                using (var jsonTextWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonTextWriter, obj);
                }

                return ms.ToArray();
            }
        }

        public TResult GetValue<TResult>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new StreamReader(ms))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<TResult>(jsonReader);
            }
        }
    }
}
