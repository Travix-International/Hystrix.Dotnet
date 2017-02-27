namespace Hystrix.Dotnet
{
    internal class RollingPercentileBucket
    {
        public long WindowStart { get; }

        public PercentileBucketData Data { get; }

        public RollingPercentileBucket(long startTime, int bucketDataLength) 
        {
            WindowStart = startTime;
            Data = new PercentileBucketData(bucketDataLength);
        }
    }
}
