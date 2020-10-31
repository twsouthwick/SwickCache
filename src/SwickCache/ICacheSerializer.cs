namespace Swick.Cache
{
    public interface ICacheSerializer
    {
        byte[] GetBytes<T>(T obj);

        T GetValue<T>(byte[] data);
    }
}
