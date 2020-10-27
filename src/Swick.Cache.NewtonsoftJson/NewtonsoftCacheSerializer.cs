using Newtonsoft.Json;
using System.IO;

namespace Swick.Cache
{
    public class NewtonsoftCacheSerializer<T> : ICacheSerializer<T>
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftCacheSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public (byte[] bytes, T result) GetBytes(T obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                using (var jsonTextWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonTextWriter, obj);
                }

                return (ms.ToArray(), obj);
            }
        }

        public T GetValue(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new StreamReader(ms))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
