namespace Swick.Cache
{
    public interface ICacheInvalidator<T>
        where T : class
    {
        /// <summary>
        /// Gets a proxy to invalidate entries that may have been cached.
        /// </summary>
        T Value { get; }
    }
}
