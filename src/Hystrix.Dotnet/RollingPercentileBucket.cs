namespace Hystrix.Dotnet
{
    internal class RollingPercentileBucket
    {
        private readonly long windowStart;
        private readonly PercentileBucketData data;

        public long WindowStart { get { return windowStart; } }

        public PercentileBucketData Data { get { return data; } }

        public RollingPercentileBucket(long startTime, int bucketDataLength) 
        {
            windowStart = startTime;
            data = new PercentileBucketData(bucketDataLength);
        }
    }
}
