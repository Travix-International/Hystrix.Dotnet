using System;
using System.Threading;

namespace Hystrix.Dotnet
{
    public class HystrixCommandMetrics : IHystrixCommandMetrics
    {
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IHystrixConfigurationService configurationService;
        private readonly HystrixRollingNumber counter;

        public IHystrixConfigurationService ConfigurationService => configurationService;

        private long lastHealthCountsSnapshot;

        private HystrixHealthCounts healthCountsSnapshot = new HystrixHealthCounts(0, 0, 0);

        private readonly HystrixRollingPercentile percentileExecution;
        private readonly HystrixRollingPercentile percentileTotal;
        private int concurrentExecutionCount;

        public HystrixCommandMetrics(IDateTimeProvider dateTimeProvider, IHystrixConfigurationService configurationService)
        {
            this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            percentileExecution = new HystrixRollingPercentile(dateTimeProvider, configurationService.GetMetricsRollingPercentileWindowInMilliseconds(), configurationService.GetMetricsRollingPercentileWindowBuckets(), configurationService.GetMetricsRollingPercentileBucketSize(), configurationService);
            percentileTotal = new HystrixRollingPercentile(dateTimeProvider, configurationService.GetMetricsRollingPercentileWindowInMilliseconds(), configurationService.GetMetricsRollingPercentileWindowBuckets(), configurationService.GetMetricsRollingPercentileBucketSize(), configurationService);

            counter = new HystrixRollingNumber(dateTimeProvider, configurationService.GetMetricsRollingStatisticalWindowInMilliseconds(), configurationService.GetMetricsRollingStatisticalWindowBuckets());
        }

        /// <inheritdoc/>
        public HystrixHealthCounts GetHealthCounts()
        {
            // we put an interval between snapshots so high-volume commands don't 
            // spend too much unnecessary time calculating metrics in very small time periods
            long lastTime = lastHealthCountsSnapshot;
            long currentTime = dateTimeProvider.CurrentTimeInMilliseconds;
            if (((currentTime - lastTime) >= configurationService.GetMetricsHealthSnapshotIntervalInMilliseconds() || healthCountsSnapshot == null) &&
                Interlocked.CompareExchange(ref lastHealthCountsSnapshot, dateTimeProvider.CurrentTimeInMilliseconds, lastTime) == lastTime)
            {
                // our thread won setting the snapshot time so we will proceed with generating a new snapshot
                // losing threads will continue using the old snapshot
                long success = counter.GetRollingSum(HystrixRollingNumberEvent.Success);
                long failure = counter.GetRollingSum(HystrixRollingNumberEvent.Failure); // fallbacks occur on this
                long timeout = counter.GetRollingSum(HystrixRollingNumberEvent.Timeout); // fallbacks occur on this

                // not used in dotnet version
                long threadPoolRejected = counter.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected);
                long semaphoreRejected = counter.GetRollingSum(HystrixRollingNumberEvent.SemaphoreRejected);

                long totalCount = failure + success + timeout + threadPoolRejected + semaphoreRejected;
                long errorCount = failure + timeout + threadPoolRejected + semaphoreRejected;
                int errorPercentage = 0;

                if (totalCount > 0)
                {
                    errorPercentage = (int)((double)errorCount / totalCount * 100);
                }

                healthCountsSnapshot = new HystrixHealthCounts(totalCount, errorCount, errorPercentage);
            }

            return healthCountsSnapshot;
        }

        public void MarkSuccess()
        {
            counter.Increment(HystrixRollingNumberEvent.Success);
        }

        public void MarkFailure()
        {
            counter.Increment(HystrixRollingNumberEvent.Failure);
        }

        public void MarkTimeout()
        {
            counter.Increment(HystrixRollingNumberEvent.Timeout);
        }

        public void MarkShortCircuited()
        {
            counter.Increment(HystrixRollingNumberEvent.ShortCircuited);
        }

        public void MarkThreadPoolRejection()
        {
            counter.Increment(HystrixRollingNumberEvent.ThreadPoolRejected);
        }

        public void MarkSemaphoreRejection()
        {
            counter.Increment(HystrixRollingNumberEvent.SemaphoreRejected);
        }

        public void MarkBadRequest()
        {
            counter.Increment(HystrixRollingNumberEvent.BadRequest);
        }

        public void IncrementConcurrentExecutionCount()
        {
            Interlocked.Increment(ref concurrentExecutionCount);
            counter.UpdateRollingMax(HystrixRollingNumberEvent.CommandMaxActive, concurrentExecutionCount);
        }

        public void DecrementConcurrentExecutionCount()
        {
            Interlocked.Decrement(ref concurrentExecutionCount);
        }

        public long GetRollingMaxConcurrentExecutions()
        {
            return counter.GetRollingMaxValue(HystrixRollingNumberEvent.CommandMaxActive);
        }

        public void MarkFallbackSuccess()
        {
            counter.Increment(HystrixRollingNumberEvent.FallbackSuccess);
        }

        public void MarkFallbackRejection()
        {
            counter.Increment(HystrixRollingNumberEvent.FallbackRejection);
        }

        public void MarkFallbackMissing()
        {
            counter.Increment(HystrixRollingNumberEvent.FallbackMissing);
        }

        public void MarkExceptionThrown()
        {
            counter.Increment(HystrixRollingNumberEvent.ExceptionThrown);
        }

        public void AddCommandExecutionTime(double duration)
        {
            percentileExecution.AddValue((int)duration);
        }

        public void AddUserThreadExecutionTime(double duration)
        {
            percentileTotal.AddValue((int)duration);
        }

        public long GetRollingSum(HystrixRollingNumberEvent type)
        {
            return counter.GetRollingSum(type);
        }

        public int GetCurrentConcurrentExecutionCount()
        {
            return concurrentExecutionCount;
        }

        public int GetExecutionTimeMean()
        {
            return percentileExecution.GetMean();
        }

        public int GetExecutionTimePercentile(double percentile)
        {
            return percentileExecution.GetPercentile(percentile);
        }

        public int GetTotalTimeMean()
        {
            return percentileTotal.GetMean();
        }

        public int GetTotalTimePercentile(double percentile)
        {
            return percentileTotal.GetPercentile(percentile);
        }

        public void ResetCounter()
        {
            counter.Reset();
            lastHealthCountsSnapshot = dateTimeProvider.CurrentTimeInMilliseconds;
            healthCountsSnapshot = new HystrixHealthCounts(0, 0, 0);
        }
    }
}
