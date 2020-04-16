using System;

namespace Swick.Cache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CachedAttribute : Attribute
    {
        public CachedAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedAttribute"/> class.
        /// Takes days, hours, and minutes parameters.  If all of the parameters
        /// are positive the duration is set accordingly; otherwise Duration does
        /// not have a value.
        /// </summary>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        public CachedAttribute(int days = 0, int hours = 0, int minutes = 0)
        {
            if (days < 0 || hours < 0 || minutes < 0
                || (days == 0 && hours == 0 && minutes == 0))
            {
                return; // so that Duration.HasValue == false holds
            }

            Duration = new TimeSpan(days: days, hours: hours, minutes: minutes, seconds: 0);
        }

        public TimeSpan? Duration { get; }
    }
}
