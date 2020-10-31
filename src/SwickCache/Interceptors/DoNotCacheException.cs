using System;

namespace Swick.Cache
{
    public class DoNotCacheException : Exception
    {
        public DoNotCacheException(string message)
            : base(message)
        {
        }
    }
}
