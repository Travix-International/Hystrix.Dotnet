using System;

namespace Hystrix.Dotnet
{
    public class HystrixLocalConfigurationService : IHystrixConfigurationService
    {
        private readonly HystrixCommandOptions options;

        public HystrixLocalConfigurationService(HystrixCommandIdentifier commandIdentifier, HystrixLocalOptions localOptions)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException(nameof(commandIdentifier));
            }

            if (localOptions == null)
            {
                throw new ArgumentNullException(nameof(localOptions), "The option Details must be provided in order to use the HystrixLocalConfigurationService.");
            }

            options = localOptions.GetCommandOptions(commandIdentifier);
        }

        /// <inheritdoc/>
        public int GetCommandTimeoutInMilliseconds() => options.CommandTimeoutInMilliseconds;

        public int GetCommandRetryCount() => options.CommandRetryCount;

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedOpen() => options.CircuitBreakerForcedOpen;

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedClosed() => options.CircuitBreakerForcedClosed;

        /// <inheritdoc/>
        public int GetCircuitBreakerErrorThresholdPercentage() => options.CircuitBreakerErrorThresholdPercentage;

        /// <inheritdoc/>
        public int GetCircuitBreakerSleepWindowInMilliseconds() => options.CircuitBreakerSleepWindowInMilliseconds;

        /// <inheritdoc/>
        public int GetCircuitBreakerRequestVolumeThreshold() => options.CircuitBreakerRequestVolumeThreshold;

        /// <inheritdoc/>
        public int GetMetricsHealthSnapshotIntervalInMilliseconds() => options.MetricsHealthSnapshotIntervalInMilliseconds;

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowInMilliseconds() => options.MetricsRollingStatisticalWindowInMilliseconds;

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowBuckets() => options.MetricsRollingStatisticalWindowBuckets;

        /// <inheritdoc/>
        public bool GetMetricsRollingPercentileEnabled() => options.MetricsRollingPercentileEnabled;

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowInMilliseconds() => options.MetricsRollingPercentileWindowInMilliseconds;

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowBuckets() => options.MetricsRollingPercentileWindowBuckets;

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileBucketSize() => options.MetricsRollingPercentileBucketSize;

        /// <inheritdoc/>
        public bool GetHystrixCommandEnabled() => options.HystrixCommandEnabled;
    }
}