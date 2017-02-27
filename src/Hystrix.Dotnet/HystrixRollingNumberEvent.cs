using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hystrix.Dotnet
{
    public enum HystrixRollingNumberEvent
    {
        // NOTE: If the set of enums changes, the logic in HystrixRollingNumberEventExtensions needs to be updated.
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

    public enum EventKind
    {
        Counter,
        MaxUpdater
    }

    public static class HystrixRollingNumberEventExtensions
    {
        private static readonly IDictionary<HystrixRollingNumberEvent, EventKind> eventKinds = new Dictionary<HystrixRollingNumberEvent, EventKind>
        {
            [HystrixRollingNumberEvent.Success] = EventKind.Counter,
            [HystrixRollingNumberEvent.Failure] = EventKind.Counter,
            [HystrixRollingNumberEvent.Timeout] = EventKind.Counter,
            [HystrixRollingNumberEvent.ShortCircuited] = EventKind.Counter,
            [HystrixRollingNumberEvent.ThreadPoolRejected] = EventKind.Counter,
            [HystrixRollingNumberEvent.SemaphoreRejected] = EventKind.Counter,
            [HystrixRollingNumberEvent.BadRequest] = EventKind.Counter,
            [HystrixRollingNumberEvent.FallbackSuccess] = EventKind.Counter,
            [HystrixRollingNumberEvent.FallbackFailure] = EventKind.Counter,
            [HystrixRollingNumberEvent.FallbackRejection] = EventKind.Counter,
            [HystrixRollingNumberEvent.FallbackMissing] = EventKind.Counter,
            [HystrixRollingNumberEvent.ExceptionThrown] = EventKind.Counter,
            [HystrixRollingNumberEvent.Emit] = EventKind.Counter,
            [HystrixRollingNumberEvent.FallbackEmit] = EventKind.Counter,
            [HystrixRollingNumberEvent.ThreadExecution] = EventKind.Counter,
            [HystrixRollingNumberEvent.Collapsed] = EventKind.Counter,
            [HystrixRollingNumberEvent.ResponseFromCache] = EventKind.Counter,
            [HystrixRollingNumberEvent.CollapserRequestBatched] = EventKind.Counter,
            [HystrixRollingNumberEvent.CollapserBatch] = EventKind.Counter,
            [HystrixRollingNumberEvent.CommandMaxActive] = EventKind.MaxUpdater,
            [HystrixRollingNumberEvent.ThreadMaxActive] = EventKind.MaxUpdater
        };

        static HystrixRollingNumberEventExtensions()
        {
            // Check if we didn't forget to set up the EventKind for any of the enum values.
            if (Enum.GetValues(typeof(HystrixRollingNumberEvent))
                .Cast<HystrixRollingNumberEvent>()
                .Any(e => !eventKinds.ContainsKey(e)))
            {
                throw new InvalidOperationException("The EventKind has to be set up for all the HystrixRollingNumberEvent values.");
            }
        }

        public static bool IsCounter(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return eventKinds[hystrixRollingNumberEvent] == EventKind.Counter;
        }

        public static bool IsMaxUpdater(this HystrixRollingNumberEvent hystrixRollingNumberEvent)
        {
            return eventKinds[hystrixRollingNumberEvent] == EventKind.MaxUpdater;
        }
    }
}