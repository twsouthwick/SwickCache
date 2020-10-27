namespace Swick.Cache
{
    public interface ICacheSerializer<T>
    {
        (byte[] bytes, T result) GetBytes(T obj);

        T GetValue(byte[] data);
    }
}
