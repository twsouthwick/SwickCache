namespace Swick.Cache.Serialization
{
    internal class ByteArraySerializer : ICacheSerializer<byte[]>
    {
        public byte[] GetBytes(byte[] obj) => obj;

        public byte[] GetValue(byte[] data) => data;

        public bool IsImmutable(byte[] input) => true;
    }
}
