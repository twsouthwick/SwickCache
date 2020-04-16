using Swick.Cache;

namespace Swick.Cache
{
    internal class ProxyCached<T> : ICached<T>
        where T : class
    {
        public ProxyCached(CachingManager manager, T instance)
        {
            Value = manager.CreateCachedProxy<T>(instance);
        }

        public T Value { get; }
    }
}
