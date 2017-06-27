using System;
using System.Threading;
using Hystrix.Dotnet.Logging;

namespace Hystrix.Dotnet
{
    public class HystrixCircuitBreaker : IHystrixCircuitBreaker
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixCircuitBreaker));

        private readonly IDateTimeProvider dateTimeProvider;
        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixConfigurationService configurationService;
        private readonly IHystrixCommandMetrics commandMetrics;

        public bool CircuitIsOpen { get; private set; }

        private long circuitOpenedOrLastTestedTime;

        public HystrixCircuitBreaker(IDateTimeProvider dateTimeProvider, HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService, IHystrixCommandMetrics commandMetrics)
        {
            this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            this.commandIdentifier = commandIdentifier ?? throw new ArgumentNullException(nameof(commandIdentifier));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.commandMetrics = commandMetrics ?? throw new ArgumentNullException(nameof(commandMetrics));
        }

        /// <inheritdoc/>
        public bool AllowRequest()
        {
            if (configurationService.GetCircuitBreakerForcedOpen())
            {
                return false;
            }
            if (configurationService.GetCircuitBreakerForcedClosed())
            {
                return true;
            }

            return !IsOpen() || AllowSingleTest();
        }

        /// <inheritdoc/>
        private bool IsOpen()
        {
            if (CircuitIsOpen)
            {
                return true;
            }

            // we're closed, so let's see if errors have made us so we should trip the circuit open
            HystrixHealthCounts healthCounts = commandMetrics.GetHealthCounts();

            // check if we are past the CircuitBreakerRequestVolumeThreshold
            if (healthCounts.GetTotalRequests() < configurationService.GetCircuitBreakerRequestVolumeThreshold())
            {
                // we are not past the minimum volume threshold for the statisticalWindow so we'll return false immediately and not calculate anything
                return false;
            }

            // if error percentage is below threshold the circuit remains closed
            if (healthCounts.GetErrorPercentage() < configurationService.GetCircuitBreakerErrorThresholdPercentage())
            {
                return false;
            }

            // failure rate is too high, trip the circuit (multiple threads can come to these lines, but do we care?)
            OpenCircuit();

            return true;
        }

        private bool AllowSingleTest()
        {
            long localCircuitOpenedOrLastTestedTime = circuitOpenedOrLastTestedTime;

            int circuitBreakerSleepWindowInMilliseconds = configurationService.GetCircuitBreakerSleepWindowInMilliseconds();

            if (// check if sleep window has passed
                CircuitIsOpen && (dateTimeProvider.CurrentTimeInMilliseconds - circuitOpenedOrLastTestedTime) > circuitBreakerSleepWindowInMilliseconds &&
                // update circuitOpenedOrLastTestedTime if it hasn't been updated by another request in the meantime
                Interlocked.CompareExchange(ref circuitOpenedOrLastTestedTime, dateTimeProvider.CurrentTimeInMilliseconds, localCircuitOpenedOrLastTestedTime) == localCircuitOpenedOrLastTestedTime)
            {
                log.InfoFormat("Allowing single test request through circuit breaker for group {0} and key {1}.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                // this thread is the first one here and can do a canary request
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void OpenCircuit()
        {
            if (!CircuitIsOpen)
            {
                log.WarnFormat("Circuit breaker for group {0} and key {1} has opened.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                CircuitIsOpen = true;
                circuitOpenedOrLastTestedTime = dateTimeProvider.CurrentTimeInMilliseconds;                
            }
        }

        /// <inheritdoc/>
        public void CloseCircuit()
        {
            if (CircuitIsOpen)
            {
                log.InfoFormat("Circuit breaker for group {0} and key {1} has closed.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                commandMetrics.ResetCounter();

                // If we have been 'open' and have a success then we want to close the circuit. This handles the 'singleTest' logic
                CircuitIsOpen = false;
            }
        }
    }
}