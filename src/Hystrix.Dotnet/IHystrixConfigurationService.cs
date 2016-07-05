namespace Hystrix.Dotnet
{
    public interface IHystrixConfigurationService
    {
        /// <summary>
        /// Returns the timeout value for the command the instances is tied to
        /// </summary>
        /// <returns></returns>
        int GetCommandTimeoutInMilliseconds();

        /// <summary>
        /// Returns whether the circuit breaker is forced open for the command the instances is tied to
        /// </summary>
        /// <returns></returns>
        bool GetCircuitBreakerForcedOpen();

        /// <summary>
        /// Returns whether the circuit breaker is forced closed for the command the instances is tied to
        /// </summary>
        /// <returns></returns>
        bool GetCircuitBreakerForcedClosed();

        /// <summary>
        /// Returns the percentage of errors at which to trip the circuit breaker for the command the instances is tied to
        /// </summary>
        /// <returns></returns>
        int GetCircuitBreakerErrorThresholdPercentage();

        /// <summary>
        /// Returns the number of milliseconds to sleep before letting a canary request through an open circuit breaker
        /// </summary>
        /// <returns></returns>
        int GetCircuitBreakerSleepWindowInMilliseconds();

        /// <summary>
        /// Returns the number of requests required for the circuit breaker to become active
        /// </summary>
        /// <returns></returns>
        int GetCircuitBreakerRequestVolumeThreshold();

        /// <summary>
        /// Returns the snapshot interval for tracking health metrics
        /// </summary>
        /// <returns></returns>
        int GetMetricsHealthSnapshotIntervalInMilliseconds();

        /// <summary>
        /// The number of milliseconds to keep track of stats
        /// </summary>
        /// <returns></returns>
        int GetMetricsRollingStatisticalWindowInMilliseconds();

        /// <summary>
        /// The number of buckets the time during which stats are tracked is divided in
        /// </summary>
        /// <returns></returns>
        int GetMetricsRollingStatisticalWindowBuckets();

        /// <summary>
        /// Indicates whether percentile metrics are collected
        /// </summary>
        /// <returns></returns>
        bool GetMetricsRollingPercentileEnabled();

        /// <summary>
        /// The total sliding window over which percentiles are collected
        /// </summary>
        /// <returns></returns>
        int GetMetricsRollingPercentileWindowInMilliseconds();

        /// <summary>
        /// The number of buckets that sliding window is split into
        /// </summary>
        /// <returns></returns>
        int GetMetricsRollingPercentileWindowBuckets();

        /// <summary>
        /// The maximum number of response times are collected in a single bucket; if more requests happen the earlier recorded values are overwritten
        /// </summary>
        /// <returns></returns>
        int GetMetricsRollingPercentileBucketSize();

        /// <summary>
        /// Indicates whether hystrix command logic is enabled or not
        /// </summary>
        /// <returns></returns>
        bool GetHystrixCommandEnabled();
    }
}