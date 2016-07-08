using System;
using System.Linq;
using System.Reflection;

namespace Hystrix.Dotnet
{
    public enum HystrixRollingNumberEvent
    {
        [IsCounter]
        Success,
        [IsCounter]
        Failure,
        [IsCounter]
        Timeout,
        [IsCounter]
        ShortCircuited,
        [IsCounter]
        ThreadPoolRejected,
        [IsCounter]
        SemaphoreRejected,
        [IsCounter]
        BadRequest,
        [IsCounter]
        FallbackSuccess,
        [IsCounter]
        FallbackFailure,
        [IsCounter]
        FallbackRejection,
        [IsCounter]
        FallbackMissing,
        [IsCounter]
        ExceptionThrown,
        [IsMaxUpdater]
        CommandMaxActive,
        [IsCounter]
        Emit,
        [IsCounter]
        FallbackEmit,
        [IsCounter]
        ThreadExecution,
        [IsMaxUpdater]
        ThreadMaxActive,
        [IsCounter]
        Collapsed,
        [IsCounter]
        ResponseFromCache,
        [IsCounter]
        CollapserRequestBatched,
        [IsCounter]
        CollapserBatch
    }

    internal class IsCounterAttribute : Attribute
    { }

    internal class IsMaxUpdaterAttribute : Attribute
    { }

    public static class HystrixRollingNumberEventExtensions
    {
        public static bool IsCounter(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return HasAttribute<IsCounterAttribute>(hystrixRollingNumberEvent);
        }

        public static bool IsMaxUpdater(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return HasAttribute<IsMaxUpdaterAttribute>(hystrixRollingNumberEvent);
        }

        private static bool HasAttribute<T>(HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            var type = typeof(HystrixRollingNumberEvent);

            var memInfo = type.GetTypeInfo().GetMember(hystrixRollingNumberEvent.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);

            return attributes.Any();
        }
    }
}
