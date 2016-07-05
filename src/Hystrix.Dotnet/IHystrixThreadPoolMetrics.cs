namespace Hystrix.Dotnet
{
    public interface IHystrixThreadPoolMetrics
    {
        int GetCurrentActiveCount();
        long GetCurrentCompletedTaskCount();
        int GetCurrentCorePoolSize();
        int GetCurrentLargestPoolSize();
        int GetCurrentMaximumPoolSize();
        int GetCurrentAvailableThreads();
        int GetCurrentPoolSize();
        long GetCurrentTaskCount();
        int GetCurrentQueueSize();
        void MarkThreadExecution();
        long GetRollingCountThreadsExecuted();
        long GetCumulativeCountThreadsExecuted();
        void MarkThreadCompletion();
        long GetRollingMaxActiveThreads();
        void MarkThreadRejection();
        long GetRollingCountThreadPoolRejected();
        IHystrixConfigurationService ConfigurationService { get; }
    }
}