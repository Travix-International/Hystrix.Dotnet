using System;

namespace Hystrix.Dotnet
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public long CurrentTimeInMilliseconds =>
            DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
    }
}
