﻿namespace Swick.Cache
{
    public interface ICached<T>
        where T : class
    {
        /// <summary>
        /// Gets a proxy to get items that may have been cached.
        /// </summary>
        T Value { get; }
    }
}
