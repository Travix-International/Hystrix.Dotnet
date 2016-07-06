using Hystrix.Dotnet.ConcurrencyUtilities;

namespace Hystrix.Dotnet
{
    public class PercentileBucketData
    {
        private readonly int length;
        private readonly int[] list; // should be AtomicIntegerArray
        private AtomicInteger index = new AtomicInteger();

        public int[] List { get { return list; } }

        public PercentileBucketData(int dataLength) 
        {
            length = dataLength;
            list = new int[dataLength];
        }

        public void AddValue(params int[] latency) 
        {
            foreach (var l in latency)
            {
                /* We just wrap around the beginning and over-write if we go past 'dataLength' as that will effectively cause us to "sample" the most recent data */
                list[index.GetAndIncrement() % length] = l;

                // TODO Alternative to AtomicInteger? The getAndIncrement may be a source of contention on high throughput circuits on large multi-core systems.
                // LongAdder isn't suited to this as it is not consistent. Perhaps a different data structure that doesn't need indexed adds?
                // A threadlocal data storage that only aggregates when fetched would be ideal. Similar to LongAdder except for accumulating lists of data.
            }
        }

        public int Length()
        {
            if (index.GetValue() > list.Length) 
            {
                return list.Length;
            }

            return index.GetValue();
        }
    }
}
