namespace Swick.Cache
{
    public interface ICacheSerializer
    {
        byte[] GetBytes(object obj);

        TResult GetValue<TResult>(byte[] data);
    }
}
