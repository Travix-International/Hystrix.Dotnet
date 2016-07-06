namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface ValueReader<out T>
    {
        T GetValue();
        T NonVolatileGetValue();
    }
}