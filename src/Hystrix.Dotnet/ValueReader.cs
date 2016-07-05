namespace Hystrix.Dotnet
{
    internal interface ValueReader<out T>
    {
        T GetValue();
        T NonVolatileGetValue();
    }
}