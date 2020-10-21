using System;

namespace Swick.Cache
{
    internal class ProxyCached<T> : ICached<T>
        where T : class
    {
        private readonly Lazy<T> _factory;

        public ProxyCached(ICachingManager manager, T instance)
        {
            _factory = new Lazy<T>(() => manager.CreateCachedProxy<T>(instance));
        }

        public T Value => _factory.Value;
    }
}
