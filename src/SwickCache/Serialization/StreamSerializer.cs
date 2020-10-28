using System.IO;

namespace Swick.Cache.Serialization
{
    internal class StreamSerializer : ICacheSerializer<Stream>
    {
        public (byte[] bytes, Stream result) GetBytes(Stream obj)
        {
            if (obj is MemoryStream ms)
            {
                var bytes = ms.ToArray();

                return (bytes, ms);
            }
            else
            {
                var m = new MemoryStream();

                obj.CopyTo(m);
                m.Position = 0;

                return (m.ToArray(), m);
            }
        }

        public Stream GetValue(byte[] data) => new MemoryStream(data);
    }
}
