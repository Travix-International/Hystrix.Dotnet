using System;

namespace Hystrix.Dotnet
{
    public class DateTimeProvider
    {
        public virtual long GetCurrentTimeInMilliseconds()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
