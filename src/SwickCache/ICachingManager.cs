namespace Swick.Cache
{
    public interface ICachingManager
    {
        T CreateCachedProxy<T>(T target) where T : class;

        T CreateInvalidatorProxy<T>() where T : class;
    }
}