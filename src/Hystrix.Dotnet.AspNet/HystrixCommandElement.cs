using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixCommandElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"].ToString(); }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("commandTimeoutInMilliseconds", IsRequired = false, DefaultValue = 1000)]
        public int CommandTimeoutInMilliseconds
        {
            get { return (int)this["commandTimeoutInMilliseconds"]; }
            set { this["commandTimeoutInMilliseconds"] = value; }
        }

        [ConfigurationProperty("circuitBreakerForcedOpen", IsRequired = false, DefaultValue = false)]
        public bool CircuitBreakerForcedOpen
        {
            get { return (bool)this["circuitBreakerForcedOpen"]; }
            set { this["circuitBreakerForcedOpen"] = value; }
        }

        [ConfigurationProperty("circuitBreakerForcedClosed", IsRequired = false, DefaultValue = false)]
        public bool CircuitBreakerForcedClosed
        {
            get { return (bool)this["circuitBreakerForcedClosed"]; }
            set { this["circuitBreakerForcedClosed"] = value; }
        }

        [ConfigurationProperty("circuitBreakerErrorThresholdPercentage", IsRequired = false, DefaultValue = 50)]
        public int CircuitBreakerErrorThresholdPercentage
        {
            get { return (int)this["circuitBreakerErrorThresholdPercentage"]; }
            set { this["circuitBreakerErrorThresholdPercentage"] = value; }
        }

        [ConfigurationProperty("circuitBreakerSleepWindowInMilliseconds", IsRequired = false, DefaultValue = 5000)]
        public int CircuitBreakerSleepWindowInMilliseconds
        {
            get { return (int)this["circuitBreakerSleepWindowInMilliseconds"]; }
            set { this["circuitBreakerSleepWindowInMilliseconds"] = value; }
        }

        [ConfigurationProperty("circuitBreakerRequestVolumeThreshold", IsRequired = false, DefaultValue = 20)]
        public int CircuitBreakerRequestVolumeThreshold
        {
            get { return (int)this["circuitBreakerRequestVolumeThreshold"]; }
            set { this["circuitBreakerRequestVolumeThreshold"] = value; }
        }

        [ConfigurationProperty("metricsHealthSnapshotIntervalInMilliseconds", IsRequired = false, DefaultValue = 500)]
        public int MetricsHealthSnapshotIntervalInMilliseconds
        {
            get { return (int)this["metricsHealthSnapshotIntervalInMilliseconds"]; }
            set { this["metricsHealthSnapshotIntervalInMilliseconds"] = value; }
        }

        [ConfigurationProperty("metricsRollingStatisticalWindowInMilliseconds", IsRequired = false, DefaultValue = 10000)]
        public int MetricsRollingStatisticalWindowInMilliseconds
        {
            get { return (int)this["metricsRollingStatisticalWindowInMilliseconds"]; }
            set { this["metricsRollingStatisticalWindowInMilliseconds"] = value; }
        }

        [ConfigurationProperty("metricsRollingStatisticalWindowBuckets", IsRequired = false, DefaultValue = 10)]
        public int MetricsRollingStatisticalWindowBuckets
        {
            get { return (int)this["metricsRollingStatisticalWindowBuckets"]; }
            set { this["metricsRollingStatisticalWindowBuckets"] = value; }
        }

        [ConfigurationProperty("metricsRollingPercentileEnabled", IsRequired = false, DefaultValue = true)]
        public bool MetricsRollingPercentileEnabled
        {
            get { return (bool)this["metricsRollingPercentileEnabled"]; }
            set { this["metricsRollingPercentileEnabled"] = value; }
        }

        [ConfigurationProperty("metricsRollingPercentileWindowInMilliseconds", IsRequired = false, DefaultValue = 60000)]
        public int MetricsRollingPercentileWindowInMilliseconds
        {
            get { return (int)this["metricsRollingPercentileWindowInMilliseconds"]; }
            set { this["metricsRollingPercentileWindowInMilliseconds"] = value; }
        }

        [ConfigurationProperty("metricsRollingPercentileWindowBuckets", IsRequired = false, DefaultValue = 6)]
        public int MetricsRollingPercentileWindowBuckets
        {
            get { return (int)this["metricsRollingPercentileWindowBuckets"]; }
            set { this["metricsRollingPercentileWindowBuckets"] = value; }
        }

        [ConfigurationProperty("metricsRollingPercentileBucketSize", IsRequired = false, DefaultValue = 100)]
        public int MetricsRollingPercentileBucketSize
        {
            get { return (int)this["metricsRollingPercentileBucketSize"]; }
            set { this["metricsRollingPercentileBucketSize"] = value; }
        }

        [ConfigurationProperty("hystrixCommandEnabled", IsRequired = false, DefaultValue = true)]
        public bool HystrixCommandEnabled
        {
            get { return (bool)this["hystrixCommandEnabled"]; }
            set { this["hystrixCommandEnabled"] = value; }
        }
    }
}