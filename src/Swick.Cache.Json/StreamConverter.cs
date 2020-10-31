using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swick.Cache.Json
{
    public class StreamConverter : JsonConverterFactory
    {
        private readonly JsonConverter<Stream> _converter = new StreamConverterImpl();

        public override bool CanConvert(System.Type typeToConvert)
            => typeof(Stream).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(System.Type typeToConvert, JsonSerializerOptions options)
            => _converter;

        private class StreamConverterImpl : JsonConverter<Stream>
        {
            public override Stream Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
            {
                var bytes = reader.GetBytesFromBase64();
                return new MemoryStream(bytes);
            }

            public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
            {
                using var ms = GetMemoryStream(value);

                if (ms.TryGetBuffer(out var buffer))
                {
                    writer.WriteBase64StringValue(buffer);
                }
                else
                {
                    writer.WriteBase64StringValue(ms.ToArray());
                }
            }


            private static MemoryStream GetMemoryStream(Stream value)
            {
                if (value is MemoryStream m)
                {
                    return m;
                }

                var ms = new MemoryStream();
                value.CopyTo(ms);
                return ms;
            }
        }
    }
}
