using System;

#if !COREFX

using System.Configuration;

#endif

namespace Hystrix.Dotnet
{
    public class HystrixWebConfigConfigurationService : IHystrixConfigurationService
    {
        private readonly HystrixCommandIdentifier commandIdentifier;

        public HystrixWebConfigConfigurationService(HystrixCommandIdentifier commandIdentifier)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException("commandIdentifier");
            }

            this.commandIdentifier = commandIdentifier;
        }

        /// <inheritdoc/>
        public int GetCommandTimeoutInMilliseconds()
        {
            return GetConfigurationValueAsInt("CommandTimeoutInMilliseconds", 1000);
        }

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedOpen()
        {
            return GetConfigurationValueAsBool("CircuitBreakerForcedOpen", false);
        }

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedClosed()
        {
            return GetConfigurationValueAsBool("CircuitBreakerForcedClosed", false);
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerErrorThresholdPercentage()
        {
            return GetConfigurationValueAsInt("CircuitBreakerErrorThresholdPercentage", 50);
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerSleepWindowInMilliseconds()
        {
            return GetConfigurationValueAsInt("CircuitBreakerSleepWindowInMilliseconds", 5000);
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerRequestVolumeThreshold()
        {
            return GetConfigurationValueAsInt("CircuitBreakerRequestVolumeThreshold", 20);
        }

        /// <inheritdoc/>
        public int GetMetricsHealthSnapshotIntervalInMilliseconds()
        {
            return GetConfigurationValueAsInt("MetricsHealthSnapshotIntervalInMilliseconds", 500);
        }

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowInMilliseconds()
        {
            return GetConfigurationValueAsInt("MetricsRollingStatisticalWindowInMilliseconds", 10000);
        }

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowBuckets()
        {
            return GetConfigurationValueAsInt("MetricsRollingStatisticalWindowBuckets", 10);
        }

        /// <inheritdoc/>
        public bool GetMetricsRollingPercentileEnabled()
        {
            return GetConfigurationValueAsBool("MetricsRollingPercentileEnabled", true);
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowInMilliseconds()
        {
            return GetConfigurationValueAsInt("MetricsRollingPercentileWindowInMilliseconds", 60000);
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowBuckets()
        {
            return GetConfigurationValueAsInt("MetricsRollingPercentileWindowBuckets", 6);
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileBucketSize()
        {
            return GetConfigurationValueAsInt("MetricsRollingPercentileBucketSize", 100);
        }

        public bool GetHystrixCommandEnabled()
        {
            return GetConfigurationValueAsBool("HystrixCommandEnabled", true);
        }

        private bool GetConfigurationValueAsBool(string configKey, bool defaultValue)
        {
            bool value;
            if (bool.TryParse(GetConfigurationValue(configKey), out value))
            {
                return value;
            }

            return defaultValue;
        }

        private int GetConfigurationValueAsInt(string configKey, int defaultValue)
        {
            int value;
            if (int.TryParse(GetConfigurationValue(configKey), out value))
            {
                return value;
            }

            return defaultValue;
        }

        private string GetConfigurationValue(string configKey)
        {
            string key = string.Format("{0}-{1}-{2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, configKey);

            return ConfigurationManager.AppSettings[key];
        }
    }
}
