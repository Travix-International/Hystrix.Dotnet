namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface VolatileValue<T> : ValueReader<T>, ValueWriter<T> { }
}