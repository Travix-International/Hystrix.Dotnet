using System;
using System.Threading;

namespace Hystrix.Dotnet
{
    public class HystrixCircuitBreaker : IHystrixCircuitBreaker
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixCircuitBreaker));

        private readonly DateTimeProvider dateTimeProvider;
        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixConfigurationService configurationService;
        private readonly IHystrixCommandMetrics commandMetrics;

        private bool circuitIsOpen;

        public bool CircuitIsOpen { get { return circuitIsOpen; } }

        private long circuitOpenedOrLastTestedTime;

        public HystrixCircuitBreaker(HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService, IHystrixCommandMetrics commandMetrics)
            :this(new DateTimeProvider(), commandIdentifier,configurationService, commandMetrics)
        {
        }

        //[Obsolete("This constructor is only use for testing in order to inject a DateTimeProvider mock")]
        public HystrixCircuitBreaker(DateTimeProvider dateTimeProvider, HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService, IHystrixCommandMetrics commandMetrics)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException("commandIdentifier");
            }
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }
            if (commandMetrics == null)
            {
                throw new ArgumentNullException("commandMetrics");
            }

            this.dateTimeProvider = dateTimeProvider;
            this.commandIdentifier = commandIdentifier;
            this.configurationService = configurationService;
            this.commandMetrics = commandMetrics;
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
            if (circuitIsOpen)
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
                circuitIsOpen && (dateTimeProvider.GetCurrentTimeInMilliseconds() - circuitOpenedOrLastTestedTime) > circuitBreakerSleepWindowInMilliseconds &&
                // update circuitOpenedOrLastTestedTime if it hasn't been updated by another request in the meantime
                Interlocked.CompareExchange(ref circuitOpenedOrLastTestedTime, dateTimeProvider.GetCurrentTimeInMilliseconds(), localCircuitOpenedOrLastTestedTime) == localCircuitOpenedOrLastTestedTime)
            {
                //Log.InfoFormat("Allowing single test request through circuit breaker for group {0} and key {1}.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                // this thread is the first one here and can do a canary request
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void OpenCircuit()
        {
            if (!circuitIsOpen)
            {
                //Log.WarnFormat("Circuit breaker for group {0} and key {1} has opened.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                circuitIsOpen = true;
                circuitOpenedOrLastTestedTime = dateTimeProvider.GetCurrentTimeInMilliseconds();                
            }
        }

        /// <inheritdoc/>
        public void CloseCircuit()
        {
            if (circuitIsOpen)
            {
                //Log.InfoFormat("Circuit breaker for group {0} and key {1} has closed.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

                commandMetrics.ResetCounter();

                // If we have been 'open' and have a success then we want to close the circuit. This handles the 'singleTest' logic
                circuitIsOpen = false;
            }
        }
    }
}