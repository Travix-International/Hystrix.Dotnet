namespace Hystrix.Dotnet
{
    public interface IHystrixCommandMetrics
    {
        IHystrixConfigurationService ConfigurationService { get; }

        /// <inheritdoc/>
        HystrixHealthCounts GetHealthCounts();

        void MarkSuccess();
        void MarkFailure();
        void MarkTimeout();
        void MarkShortCircuited();
        void MarkThreadPoolRejection();
        void MarkSemaphoreRejection();
        void MarkBadRequest();
        void IncrementConcurrentExecutionCount();
        void DecrementConcurrentExecutionCount();
        long GetRollingMaxConcurrentExecutions();
        void MarkFallbackSuccess();
        void MarkFallbackRejection();
        void MarkFallbackMissing();
        void MarkExceptionThrown();
        void AddCommandExecutionTime(double duration);
        void AddUserThreadExecutionTime(double duration);
        long GetRollingSum(HystrixRollingNumberEvent type);
        int GetCurrentConcurrentExecutionCount();
        int GetExecutionTimeMean();
        int GetExecutionTimePercentile(double percentile);
        int GetTotalTimeMean();
        int GetTotalTimePercentile(double percentile);
        void ResetCounter();
    }
}