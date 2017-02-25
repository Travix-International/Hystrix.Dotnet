namespace Hystrix.Dotnet
{
    public enum HystrixRollingNumberEvent
    {
        // NOTE: If the set of enums changes, the numbers in HystrixRollingNumberEventExtensions need to be updated.
        // Counter events
        Success = 0,
        Failure = 1,
        Timeout = 2,
        ShortCircuited = 3,
        ThreadPoolRejected = 4,
        SemaphoreRejected = 5,
        BadRequest = 6,
        FallbackSuccess = 7,
        FallbackFailure = 8,
        FallbackRejection = 9,
        FallbackMissing = 10,
        ExceptionThrown = 11,
        Emit = 12,
        FallbackEmit = 13,
        ThreadExecution = 14,
        Collapsed = 15,
        ResponseFromCache = 16,
        CollapserRequestBatched = 17,
        CollapserBatch = 18,
        // MaxUpdater events
        CommandMaxActive = 19,
        ThreadMaxActive = 20
    }

    public static class HystrixRollingNumberEventExtensions
    {
        // NOTE: Hacky, but performant approach. Won't work if we'll have more than two kinds of events.
        public static bool IsCounter(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return (int)hystrixRollingNumberEvent <= 18;
        }

        public static bool IsMaxUpdater(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return (int)hystrixRollingNumberEvent >= 19;
        }
    }
}