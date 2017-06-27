using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixCommandElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get => this["key"].ToString();
            set => this["key"] = value;
        }

        [ConfigurationProperty("commandTimeoutInMilliseconds", IsRequired = false, DefaultValue = 1000)]
        public int CommandTimeoutInMilliseconds
        {
            get => (int)this["commandTimeoutInMilliseconds"];
            set => this["commandTimeoutInMilliseconds"] = value;
        }

        [ConfigurationProperty("circuitBreakerForcedOpen", IsRequired = false, DefaultValue = false)]
        public bool CircuitBreakerForcedOpen
        {
            get => (bool)this["circuitBreakerForcedOpen"];
            set => this["circuitBreakerForcedOpen"] = value;
        }

        [ConfigurationProperty("circuitBreakerForcedClosed", IsRequired = false, DefaultValue = false)]
        public bool CircuitBreakerForcedClosed
        {
            get => (bool)this["circuitBreakerForcedClosed"];
            set => this["circuitBreakerForcedClosed"] = value;
        }

        [ConfigurationProperty("circuitBreakerErrorThresholdPercentage", IsRequired = false, DefaultValue = 50)]
        public int CircuitBreakerErrorThresholdPercentage
        {
            get => (int)this["circuitBreakerErrorThresholdPercentage"];
            set => this["circuitBreakerErrorThresholdPercentage"] = value;
        }

        [ConfigurationProperty("circuitBreakerSleepWindowInMilliseconds", IsRequired = false, DefaultValue = 5000)]
        public int CircuitBreakerSleepWindowInMilliseconds
        {
            get => (int)this["circuitBreakerSleepWindowInMilliseconds"];
            set => this["circuitBreakerSleepWindowInMilliseconds"] = value;
        }

        [ConfigurationProperty("circuitBreakerRequestVolumeThreshold", IsRequired = false, DefaultValue = 20)]
        public int CircuitBreakerRequestVolumeThreshold
        {
            get => (int)this["circuitBreakerRequestVolumeThreshold"];
            set => this["circuitBreakerRequestVolumeThreshold"] = value;
        }

        [ConfigurationProperty("metricsHealthSnapshotIntervalInMilliseconds", IsRequired = false, DefaultValue = 500)]
        public int MetricsHealthSnapshotIntervalInMilliseconds
        {
            get => (int)this["metricsHealthSnapshotIntervalInMilliseconds"];
            set => this["metricsHealthSnapshotIntervalInMilliseconds"] = value;
        }

        [ConfigurationProperty("metricsRollingStatisticalWindowInMilliseconds", IsRequired = false, DefaultValue = 10000)]
        public int MetricsRollingStatisticalWindowInMilliseconds
        {
            get => (int)this["metricsRollingStatisticalWindowInMilliseconds"];
            set => this["metricsRollingStatisticalWindowInMilliseconds"] = value;
        }

        [ConfigurationProperty("metricsRollingStatisticalWindowBuckets", IsRequired = false, DefaultValue = 10)]
        public int MetricsRollingStatisticalWindowBuckets
        {
            get => (int)this["metricsRollingStatisticalWindowBuckets"];
            set => this["metricsRollingStatisticalWindowBuckets"] = value;
        }

        [ConfigurationProperty("metricsRollingPercentileEnabled", IsRequired = false, DefaultValue = true)]
        public bool MetricsRollingPercentileEnabled
        {
            get => (bool)this["metricsRollingPercentileEnabled"];
            set => this["metricsRollingPercentileEnabled"] = value;
        }

        [ConfigurationProperty("metricsRollingPercentileWindowInMilliseconds", IsRequired = false, DefaultValue = 60000)]
        public int MetricsRollingPercentileWindowInMilliseconds
        {
            get => (int)this["metricsRollingPercentileWindowInMilliseconds"];
            set => this["metricsRollingPercentileWindowInMilliseconds"] = value;
        }

        [ConfigurationProperty("metricsRollingPercentileWindowBuckets", IsRequired = false, DefaultValue = 6)]
        public int MetricsRollingPercentileWindowBuckets
        {
            get => (int)this["metricsRollingPercentileWindowBuckets"];
            set => this["metricsRollingPercentileWindowBuckets"] = value;
        }

        [ConfigurationProperty("metricsRollingPercentileBucketSize", IsRequired = false, DefaultValue = 100)]
        public int MetricsRollingPercentileBucketSize
        {
            get => (int)this["metricsRollingPercentileBucketSize"];
            set => this["metricsRollingPercentileBucketSize"] = value;
        }

        [ConfigurationProperty("hystrixCommandEnabled", IsRequired = false, DefaultValue = true)]
        public bool HystrixCommandEnabled
        {
            get => (bool)this["hystrixCommandEnabled"];
            set => this["hystrixCommandEnabled"] = value;
        }
    }
}