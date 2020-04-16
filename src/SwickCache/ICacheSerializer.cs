namespace Swick.Cache
{
    public interface ICacheSerializer
    {
        byte[] GetBytes<T>(T obj);

        TResult GetValue<TResult>(byte[] data);
    }
}
