using System;
using System.Threading;

namespace Hystrix.Dotnet
{
    public class HystrixThreadPoolMetrics : IHystrixThreadPoolMetrics
    {
        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixConfigurationService configurationService;
        private readonly HystrixRollingNumber counter;

        public IHystrixConfigurationService ConfigurationService { get { return configurationService; } }

        public HystrixThreadPoolMetrics(HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException("commandIdentifier");
            }
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }
            this.commandIdentifier = commandIdentifier;
            this.configurationService = configurationService;

            counter = new HystrixRollingNumber(configurationService.GetMetricsRollingStatisticalWindowInMilliseconds(), configurationService.GetMetricsRollingStatisticalWindowBuckets());
        }

        public int GetCurrentActiveCount()
        {
            //return threadPool.getActiveCount();
            return GetCurrentMaximumPoolSize() - GetCurrentAvailableThreads();
        }

        public long GetCurrentCompletedTaskCount()
        {
            //return threadPool.getCompletedTaskCount();
            return 0;
        }

        public int GetCurrentCorePoolSize()
        {
            //return threadPool.getCorePoolSize();
            return GetCurrentMaximumPoolSize();
        }

        public int GetCurrentLargestPoolSize()
        {
            //return threadPool.getLargestPoolSize();
            return 0;
        }

        public int GetCurrentMaximumPoolSize()
        {
            //return threadPool.getMaximumPoolSize();
            int maxWorkerThreads;

            #if !COREFX
            int maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
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
            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);
            #else
            availableWorkerThreads = 0;
            #endif

            return availableWorkerThreads;
        }

        public int GetCurrentPoolSize()
        {
            //return threadPool.getPoolSize();
            return GetCurrentMaximumPoolSize();
        }

        public long GetCurrentTaskCount()
        {
            //return threadPool.getTaskCount();
            return 0;
        }

        public int GetCurrentQueueSize()
        {
            //return threadPool.getQueue().size();
            return 0;
        }

        public void MarkThreadExecution()
        {
            // increment the count
            //counter.Increment(HystrixRollingNumberEvent.ThreadExecution);
            //SetMaxActiveThreads();
        }

        public long GetRollingCountThreadsExecuted()
        {
            //return getRollingCount(HystrixRollingNumberEvent.ThreadExecution);
            return counter.GetRollingSum(HystrixRollingNumberEvent.ThreadExecution);
        }

        public long GetRollingCountThreadPoolRejected()
        {
            return counter.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected);
        }

        public long GetCumulativeCountThreadsExecuted()
        {
            //return getCumulativeCount(HystrixRollingNumberEvent.ThreadExecution);
            return 0;
        }

        public void MarkThreadCompletion()
        {
            //SetMaxActiveThreads();
        }

        public long GetRollingMaxActiveThreads()
        {
            return counter.GetRollingMaxValue(HystrixRollingNumberEvent.ThreadMaxActive);
        }

        private void SetMaxActiveThreads()
        {
            counter.UpdateRollingMax(HystrixRollingNumberEvent.ThreadMaxActive, GetCurrentActiveCount());
        }

        public void MarkThreadRejection()
        {
            counter.Increment(HystrixRollingNumberEvent.ThreadPoolRejected);
        }
    }
}