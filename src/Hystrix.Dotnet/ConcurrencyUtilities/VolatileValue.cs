namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface IVolatileValue<T> : IValueReader<T>, IValueWriter<T> { }
}