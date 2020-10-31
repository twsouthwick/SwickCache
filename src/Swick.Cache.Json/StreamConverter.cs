using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swick.Cache
{
    internal class StreamConverter : JsonConverterFactory
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
                var ms = new MemoryStream();
                value.CopyTo(ms);
                writer.WriteBase64StringValue(ms.ToArray());
            }
        }
    }
}
