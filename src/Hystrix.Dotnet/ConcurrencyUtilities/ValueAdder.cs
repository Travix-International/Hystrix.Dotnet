namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    internal interface ValueAdder<T> : ValueReader<T>
    {
        T GetAndReset();
        void Add(T value);
        void Increment();
        void Increment(T value);
        void Decrement();
        void Decrement(T value);
        void Reset();
    }
}