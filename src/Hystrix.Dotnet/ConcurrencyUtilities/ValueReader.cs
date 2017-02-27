namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface IValueReader<out T>
    {
        T GetValue();
        T NonVolatileGetValue();
    }
}