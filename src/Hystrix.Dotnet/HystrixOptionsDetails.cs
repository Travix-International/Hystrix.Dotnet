namespace Hystrix.Dotnet
{
    public class HystrixCommandOptions
    {
        public static HystrixCommandOptions CreateDefault()
        {
            return new HystrixCommandOptions();
        }

        public int CommandTimeoutInMilliseconds { get; set; } = 1000;

        public int CommandRetryCount { get; set; } = 0;

        public bool CircuitBreakerForcedOpen { get; set; } = false;

        public bool CircuitBreakerForcedClosed { get; set; } = false;

        public int CircuitBreakerErrorThresholdPercentage { get; set; } = 50;

        public int CircuitBreakerSleepWindowInMilliseconds { get; set; } = 5000;

        public int CircuitBreakerRequestVolumeThreshold { get; set; } = 20;

        public int MetricsHealthSnapshotIntervalInMilliseconds { get; set; } = 500;

        public int MetricsRollingStatisticalWindowInMilliseconds { get; set; } = 10000;

        public int MetricsRollingStatisticalWindowBuckets { get; set; } = 10;

        public bool MetricsRollingPercentileEnabled { get; set; } = true;

        public int MetricsRollingPercentileWindowInMilliseconds { get; set; } = 60000;

        public int MetricsRollingPercentileWindowBuckets { get; set; } = 6;

        public int MetricsRollingPercentileBucketSize { get; set; } = 100;

        public bool HystrixCommandEnabled { get; set; } = true;
    }
}