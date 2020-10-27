using System.Runtime.CompilerServices;

namespace Swick.Cache.Serialization
{
    internal class ByteArraySerializer : ICacheSerializer<byte[]>
    {
        public (byte[] bytes, byte[] result) GetBytes(byte[] obj)
            => (obj, obj);

        public byte[] GetValue(byte[] data) => data;
    }
}
