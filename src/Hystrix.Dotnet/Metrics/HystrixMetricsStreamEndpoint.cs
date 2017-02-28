using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Hystrix.Dotnet.Metrics
{
    public class HystrixMetricsStreamEndpoint : IHystrixMetricsStreamEndpoint
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HystrixMetricsStreamEndpoint));

        private static readonly DateTimeProvider dateTimeProvider = new DateTimeProvider();

        private static readonly JsonSerializer jsonSerializer = JsonSerializer.Create();

        private readonly IHystrixCommandFactory commandFactory;

        private readonly int pollingInterval;

        public HystrixMetricsStreamEndpoint(IHystrixCommandFactory commandFactory, HystrixOptions options) :
            this(commandFactory, options?.MetricsStreamPollIntervalInMilliseconds ?? 500)
        {
        }

        public HystrixMetricsStreamEndpoint(IHystrixCommandFactory commandFactory, int pollingInterval)
        {
            if (commandFactory == null)
            {
                throw new ArgumentNullException(nameof(commandFactory));
            }

            if (pollingInterval < 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Parameter pollingInterval needs to be greater than or equal to 100");
            }

            this.commandFactory = commandFactory;
            this.pollingInterval = pollingInterval;
        }

        public async Task PushContentToOutputStream(Stream outputStream, Action flushResponse, CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    log.Info("Start writing to Hystrix outputstream");

                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await WriteAllCommandsJsonToOutputStream(outputStream, cancellationToken).ConfigureAwait(false);
                        await outputStream.FlushAsync(cancellationToken);

                        flushResponse();

                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    log.Info("Flushing and closing Hystrix outputstream");

                    // Close output stream as we are done
                    await outputStream.FlushAsync(cancellationToken);
                    flushResponse();
                }
            }
            catch (TaskCanceledException)
            {
                // This just means that the connection was closed.
            }
        }

        public async Task WriteAllCommandsJsonToOutputStream(Stream outputStream, CancellationToken cancellationToken)
        {
            var commands = commandFactory.GetAllHystrixCommands();

            foreach (var commandMetric in commands)
            {
                await WriteStringToOutputStream(outputStream, "data:", cancellationToken);
                WriteCommandJson(commandMetric, outputStream);
                await WriteStringToOutputStream(outputStream, "\n\n", cancellationToken);
            }

            if (!commands.Any())
            {
                await WriteStringToOutputStream(outputStream, "ping: \n", cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task WriteStringToOutputStream(Stream outputStream, string wrappedJsonString, CancellationToken cancellationToken)
        {
            // NOTE: The false argument to the UTF8Encoding constructor is important, otherwise it would embed BOMs into the stream.
            using (var sw = new StreamWriter(outputStream, new UTF8Encoding(false), 1024, true))
            {
                await sw.WriteAsync(wrappedJsonString).ConfigureAwait(false);
                await sw.FlushAsync().ConfigureAwait(false);
            }

            await outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public void WriteCommandJson(IHystrixCommand command, Stream outputStream)
        {
            var commandIdentifier = command.CommandIdentifier;
            var circuitBreaker = command.CircuitBreaker;
            var commandMetrics = command.CommandMetrics;
            var configurationService = commandMetrics.ConfigurationService;

            var healthCounts = commandMetrics.GetHealthCounts();

            // NOTE: The false argument to the UTF8Encoding constructor is important, otherwise it would embed BOMs into the stream.
            using (var sw = new StreamWriter(outputStream, new UTF8Encoding(false), 1024, true))
            {
                jsonSerializer.Serialize(sw, new
                {
                    type = "HystrixCommand",
                    name = commandIdentifier.CommandKey,
                    group = commandIdentifier.GroupKey,
                    currentTime = dateTimeProvider.GetCurrentTimeInMilliseconds(),
                    isCircuitBreakerOpen = circuitBreaker != null && circuitBreaker.CircuitIsOpen,

                    errorPercentage = healthCounts.GetErrorPercentage(),
                    errorCount = healthCounts.GetErrorCount(),
                    requestCount = healthCounts.GetTotalRequests(),

                    // rolling counters
                    rollingCountBadRequests = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.BadRequest),
                    rollingCountCollapsedRequests = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.Collapsed),
                    rollingCountEmit = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.Emit),
                    rollingCountExceptionsThrown = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.ExceptionThrown),
                    rollingCountFailure = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.Failure),
                    rollingCountFallbackEmit = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.FallbackEmit),
                    rollingCountFallbackFailure = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.FallbackFailure),
                    rollingCountFallbackMissing = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.FallbackMissing),
                    rollingCountFallbackRejection = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.FallbackRejection),
                    rollingCountFallbackSuccess = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.FallbackSuccess),
                    rollingCountResponsesFromCache = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.ResponseFromCache),
                    rollingCountSemaphoreRejected = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.SemaphoreRejected),
                    rollingCountShortCircuited = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited),
                    rollingCountSuccess = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.Success),
                    rollingCountThreadPoolRejected = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected),
                    rollingCountTimeout = commandMetrics.GetRollingSum(HystrixRollingNumberEvent.Timeout),
                    currentConcurrentExecutionCount = commandMetrics.GetCurrentConcurrentExecutionCount(),
                    rollingMaxConcurrentExecutionCount = commandMetrics.GetRollingMaxConcurrentExecutions(),

                    // latency percentiles
                    latencyExecute_mean = commandMetrics.GetExecutionTimeMean(),

                    latencyExecute = new Dictionary<string, int>
                    {
                        {"0", commandMetrics.GetExecutionTimePercentile(0)},
                        {"25", commandMetrics.GetExecutionTimePercentile(25)},
                        {"50", commandMetrics.GetExecutionTimePercentile(50)},
                        {"75", commandMetrics.GetExecutionTimePercentile(75)},
                        {"90", commandMetrics.GetExecutionTimePercentile(90)},
                        {"95", commandMetrics.GetExecutionTimePercentile(95)},
                        {"99", commandMetrics.GetExecutionTimePercentile(99)},
                        {"99.5", commandMetrics.GetExecutionTimePercentile(99.5)},
                        {"100", commandMetrics.GetExecutionTimePercentile(100)}
                    },

                    latencyTotal_mean = commandMetrics.GetTotalTimeMean(),

                    latencyTotal = new Dictionary<string, int>
                    {
                        {"0", commandMetrics.GetTotalTimePercentile(0)},
                        {"25", commandMetrics.GetTotalTimePercentile(25)},
                        {"50", commandMetrics.GetTotalTimePercentile(50)},
                        {"75", commandMetrics.GetTotalTimePercentile(75)},
                        {"90", commandMetrics.GetTotalTimePercentile(90)},
                        {"95", commandMetrics.GetTotalTimePercentile(95)},
                        {"99", commandMetrics.GetTotalTimePercentile(99)},
                        {"99.5", commandMetrics.GetTotalTimePercentile(99.5)},
                        {"100", commandMetrics.GetTotalTimePercentile(100)}
                    },

                    // property values for reporting what is actually seen by the command rather than what was set somewhere
                    propertyValue_circuitBreakerRequestVolumeThreshold = configurationService.GetCircuitBreakerRequestVolumeThreshold(),
                    propertyValue_circuitBreakerSleepWindowInMilliseconds = configurationService.GetCircuitBreakerSleepWindowInMilliseconds(),
                    propertyValue_circuitBreakerErrorThresholdPercentage = configurationService.GetCircuitBreakerErrorThresholdPercentage(),
                    propertyValue_circuitBreakerForceOpen = configurationService.GetCircuitBreakerForcedOpen(),
                    propertyValue_circuitBreakerForceClosed = configurationService.GetCircuitBreakerForcedClosed(),
                    propertyValue_circuitBreakerEnabled = configurationService.GetHystrixCommandEnabled(),

                    propertyValue_executionIsolationStrategy = "THREAD", // configurationService.GetExecutionIsolationStrategy().Name,
                    propertyValue_executionIsolationThreadTimeoutInMilliseconds = 0, //configurationService.GetExecutionTimeoutInMilliseconds(),
                    propertyValue_executionTimeoutInMilliseconds = configurationService.GetCommandTimeoutInMilliseconds(),
                    propertyValue_executionIsolationThreadInterruptOnTimeout = false, //configurationService.GetExecutionIsolationThreadInterruptOnTimeout(),
                    propertyValue_executionIsolationThreadPoolKeyOverride = (string)null, // configurationService.GetExecutionIsolationThreadPoolKeyOverride(),
                    propertyValue_executionIsolationSemaphoreMaxConcurrentRequests = 0, // configurationService.GetExecutionIsolationSemaphoreMaxConcurrentRequests(),
                    propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests = 0, // configurationService.GetFallbackIsolationSemaphoreMaxConcurrentRequests(),

                    /*
                    * The following are commented out as these rarely change and are verbose for streaming for something people don't change.
                    * We could perhaps allow a property or request argument to include these.
                    */

                    propertyValue_metricsRollingPercentileEnabled = configurationService.GetMetricsRollingPercentileEnabled(),
                    propertyValue_metricsRollingPercentileBucketSize = configurationService.GetMetricsRollingPercentileBucketSize(),
                    propertyValue_metricsRollingPercentileWindow = configurationService.GetMetricsRollingPercentileWindowInMilliseconds(),
                    propertyValue_metricsRollingPercentileWindowBuckets = configurationService.GetMetricsRollingPercentileWindowBuckets(),
                    propertyValue_metricsRollingStatisticalWindowBuckets = configurationService.GetMetricsRollingStatisticalWindowBuckets(),
                    propertyValue_metricsRollingStatisticalWindowInMilliseconds = configurationService.GetMetricsRollingStatisticalWindowInMilliseconds(),

                    propertyValue_requestCacheEnabled = false, // configurationService.requestCacheEnabled().get(),
                    propertyValue_requestLogEnabled = false, // configurationService.requestLogEnabled().get(),

                    reportingHosts = 1, // this will get summed across all instances in a cluster
                });
            }
        }
    }
}