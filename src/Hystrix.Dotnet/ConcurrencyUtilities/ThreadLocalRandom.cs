using System;
using System.Threading;

namespace Hystrix.Dotnet.ConcurrencyUtilities
{
     /// <summary>
    /// Helper class to generate Random values is a thread safe way. Not suitable for cryptographic operations.
    /// </summary>
    public static class ThreadLocalRandom
    {
        private static readonly ThreadLocal<Random> localRandom = new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId));

        public static int Next() { return localRandom.Value.Next(); }
        public static int Next(int maxValue) { return localRandom.Value.Next(maxValue); }
        public static int Next(int minValue, int maxValue) { return localRandom.Value.Next(minValue, maxValue); }
        public static void NextBytes(byte[] buffer) { localRandom.Value.NextBytes(buffer); }
        public static double NextDouble() { return localRandom.Value.NextDouble(); }

        public static long NextLong()
        {
            long heavy = localRandom.Value.Next();
            long light = localRandom.Value.Next();
            return heavy << 32 | light;
        }

        public static long NextLong(long max)
        {
            if (max == 0)
            {
                return 0;
            }

            const int bitsPerLong = 63;
            long bits, val;
            do
            {
                bits = NextLong() & (~(1L << bitsPerLong));
                val = bits % max;
            } while (bits - val + (max - 1) < 0L);
            return val;
        }
    }
}