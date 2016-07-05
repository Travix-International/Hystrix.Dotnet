namespace Hystrix.Dotnet
{
    internal interface VolatileValue<T> : ValueReader<T>, ValueWriter<T> { }
}