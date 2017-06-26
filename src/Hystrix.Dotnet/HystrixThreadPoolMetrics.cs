using System;

namespace Hystrix.Dotnet
{
    public class HystrixThreadPoolMetrics : IHystrixThreadPoolMetrics
    {
        private readonly HystrixRollingNumber counter;

        public IHystrixConfigurationService ConfigurationService { get; }

        public HystrixThreadPoolMetrics(IDateTimeProvider dateTimeProvider, IHystrixConfigurationService configurationService)
        {
            ConfigurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            counter = new HystrixRollingNumber(dateTimeProvider, configurationService.GetMetricsRollingStatisticalWindowInMilliseconds(), configurationService.GetMetricsRollingStatisticalWindowBuckets());
        }

        public int GetCurrentActiveCount()
        {
            return GetCurrentMaximumPoolSize() - GetCurrentAvailableThreads();
        }

        public long GetCurrentCompletedTaskCount()
        {
            return 0;
        }

        public int GetCurrentCorePoolSize()
        {
            return GetCurrentMaximumPoolSize();
        }

        public int GetCurrentLargestPoolSize()
        {
            return 0;
        }

        public int GetCurrentMaximumPoolSize()
        {
            int maxWorkerThreads;

            #if !COREFX
            int maxCompletionPortThreads;
            System.Threading.ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            #else
            maxWorkerThreads = 0;
            #endif

            return maxWorkerThreads;
        }

        public int GetCurrentAvailableThreads()
        {
            int availableWorkerThreads;

            #if !COREFX
            int availableCompletionPortThreads;
            System.Threading.ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);
            #else
            availableWorkerThreads = 0;
            #endif

            return availableWorkerThreads;
        }

        public int GetCurrentPoolSize()
        {
            return GetCurrentMaximumPoolSize();
        }

        public long GetCurrentTaskCount()
        {
            return 0;
        }

        public int GetCurrentQueueSize()
        {
            return 0;
        }

        public void MarkThreadExecution()
        {
        }

        public long GetRollingCountThreadsExecuted()
        {
            return counter.GetRollingSum(HystrixRollingNumberEvent.ThreadExecution);
        }

        public long GetRollingCountThreadPoolRejected()
        {
            return counter.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected);
        }

        public long GetCumulativeCountThreadsExecuted()
        {
            return 0;
        }

        public void MarkThreadCompletion()
        {
        }

        public long GetRollingMaxActiveThreads()
        {
            return counter.GetRollingMaxValue(HystrixRollingNumberEvent.ThreadMaxActive);
        }

        public void MarkThreadRejection()
        {
            counter.Increment(HystrixRollingNumberEvent.ThreadPoolRejected);
        }
    }
}