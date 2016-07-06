namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface ValueWriter<in T>
    {
        void SetValue(T newValue);
        void LazySetValue(T newValue);
        void NonVolatileSetValue(T newValue);
    }
}