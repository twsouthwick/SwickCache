using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace Swick.Cache
{
    public abstract class DefaultSerializer : ICacheSerializer
    {
        public byte[] GetBytes<T>(T obj)
        {
            if (TryGetBytes(obj, out var result))
            {
                return result;
            }

            throw new NotSupportedException();
        }

        public TResult GetValue<TResult>(byte[] data)
        {
            if (TryGetValue<TResult>(data, out var result))
            {
                return result;
            }

            throw new NotSupportedException();
        }

        protected virtual bool TryGetValue<T>(byte[] data, out T value)
        {
            if (typeof(T) == typeof(byte[]))
            {
                value = (T)(object)data;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        protected virtual bool TryGetBytes<T>(T obj, out byte[] bytes)
        {
            if (typeof(T) == typeof(byte[]))
            {
                bytes = (byte[])(object)obj;
                return true;
            }
            else
            {
                bytes = null;
                return false;
            }
        }
    }
}
