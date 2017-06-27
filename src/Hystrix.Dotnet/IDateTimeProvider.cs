namespace Hystrix.Dotnet
{
    public interface IDateTimeProvider
    {
        long CurrentTimeInMilliseconds { get; }
    }
}
