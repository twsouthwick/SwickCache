using System;

namespace Swick.Cache
{
    public interface ICacheInvalidator
    {
        DateTimeOffset Expiration { get; }
    }
}
