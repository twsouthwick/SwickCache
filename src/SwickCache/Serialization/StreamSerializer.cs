using System.IO;

namespace Swick.Cache.Serialization
{
    internal class StreamSerializer : ICacheSerializer<Stream>
    {
        public bool IsImmutable(Stream input) => input.CanSeek;

        public byte[] GetBytes(Stream obj)
        {
            if (obj is MemoryStream ms)
            {
                return ms.ToArray();
            }
            else
            {
                using var m = new MemoryStream();

                var position = obj.Position;

                obj.CopyTo(m);

                if (IsImmutable(obj))
                {
                    obj.Seek(position, SeekOrigin.Begin);
                }

                return m.ToArray();
            }
        }

        public Stream GetValue(byte[] data) => new MemoryStream(data);
    }
}
