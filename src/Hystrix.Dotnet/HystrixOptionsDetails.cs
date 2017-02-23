namespace Hystrix.Dotnet
{
    public class HystrixCommandOptions
    {
        public HystrixCommandOptions()
        {
            CommandTimeoutInMilliseconds = 1000;
            CircuitBreakerForcedOpen = false;
            CircuitBreakerForcedClosed = false;
            CircuitBreakerErrorThresholdPercentage = 50;
            CircuitBreakerSleepWindowInMilliseconds = 5000;
            CircuitBreakerRequestVolumeThreshold = 20;
            MetricsHealthSnapshotIntervalInMilliseconds = 500;
            MetricsRollingStatisticalWindowInMilliseconds = 10000;
            MetricsRollingStatisticalWindowBuckets = 10;
            MetricsRollingPercentileEnabled = true;
            MetricsRollingPercentileWindowInMilliseconds = 60000;
            MetricsRollingPercentileWindowBuckets = 6;
            MetricsRollingPercentileBucketSize = 100;
            HystrixCommandEnabled = true;
        }

        public static HystrixCommandOptions CreateDefault()
        {
            return new HystrixCommandOptions();
        }

        public int CommandTimeoutInMilliseconds { get; set; }

        public bool CircuitBreakerForcedOpen { get; set; }

        public bool CircuitBreakerForcedClosed { get; set; }

        public int CircuitBreakerErrorThresholdPercentage { get; set; }

        public int CircuitBreakerSleepWindowInMilliseconds { get; set; }

        public int CircuitBreakerRequestVolumeThreshold { get; set; }

        public int MetricsHealthSnapshotIntervalInMilliseconds { get; set; }

        public int MetricsRollingStatisticalWindowInMilliseconds { get; set; }

        public int MetricsRollingStatisticalWindowBuckets { get; set; }

        public bool MetricsRollingPercentileEnabled { get; set; }

        public int MetricsRollingPercentileWindowInMilliseconds { get; set; }

        public int MetricsRollingPercentileWindowBuckets { get; set; }

        public int MetricsRollingPercentileBucketSize { get; set; }

        public bool HystrixCommandEnabled { get; set; }
    }
}