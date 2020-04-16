namespace Swick.Cache
{
    internal class CacheInvalidator<T> : ICacheInvalidator<T>
        where T : class
    {
        public CacheInvalidator(CachingManager manager)
        {
            Value = manager.CreateInvalidatorProxy<T>();
        }

        public T Value { get; }
    }
}
