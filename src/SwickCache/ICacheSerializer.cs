namespace Swick.Cache
{
    public interface ICacheSerializer
    {
        (byte[] bytes, T result) GetBytes<T>(T obj);

        T GetValue<T>(byte[] data);
    }
}
