using System;

namespace Swick.Cache
{
    public class CacheException : Exception
    {
        public CacheException(string message)
            : base(message)
        {
        }
    }
}
