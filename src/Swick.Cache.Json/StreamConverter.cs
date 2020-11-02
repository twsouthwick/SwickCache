using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swick.Cache.Json
{
    public class StreamConverter : JsonConverterFactory
    {
        private readonly JsonConverter<Stream> _converter;

        public StreamConverter()
        {
            _converter = new StreamConverterImpl();
        }

        public override bool CanConvert(Type typeToConvert)
            => typeof(Stream).IsAssignableFrom(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => _converter;

        private class StreamConverterImpl : JsonConverter<Stream>
        {
            public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var bytes = reader.GetBytesFromBase64();
                return new MemoryStream(bytes);
            }

            public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
            {
                if (value is MemoryStream m && m.TryGetBuffer(out var mbuffer))
                {
                    writer.WriteBase64StringValue(mbuffer);
                }
                else
                {
                    var buffer = ArrayPool<byte>.Shared.Rent((int)value.Length);

                    try
                    {
                        using var ms = new MemoryStream(buffer);
                        value.CopyTo(ms);
                        writer.WriteBase64StringValue(new ReadOnlySpan<byte>(buffer, 0, (int)value.Length));
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                    }
                }
            }
        }
    }
}
