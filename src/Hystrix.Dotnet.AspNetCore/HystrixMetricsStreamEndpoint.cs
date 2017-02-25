using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Hystrix.Dotnet.AspNetCore
{
    public class HystrixMetricsStreamEndpoint : IHystrixMetricsStreamEndpoint
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HystrixMetricsStreamEndpoint));

        private static readonly DateTimeProvider dateTimeProvider = new DateTimeProvider();

        private readonly IHystrixCommandFactory commandFactory;

        private readonly int pollingInterval;

        public HystrixMetricsStreamEndpoint(IHystrixCommandFactory commandFactory, IOptions<HystrixOptions> options)
        {
            if (commandFactory == null)
            {
                throw new ArgumentNullException(nameof(commandFactory));
            }

            pollingInterval = options?.Value?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            if (pollingInterval < 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Parameter pollingInterval needs to be greater than or equal to 100");
            }

            this.commandFactory = commandFactory;
        }

        public async Task PushContentToOutputStream(HttpResponse response, CancellationToken cancellationToken)
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

                    await WriteAllCommandsJsonToOutputStream(response).ConfigureAwait(false);

                    await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // This just means the connection was closed.
            }
            catch (Exception ex)
            {
                log.Error("An error occured in Hystrix outputstream", ex);
                Console.WriteLine("An error occured in Hystrix outputstream {0}", ex);
            }
        }

        public async Task WriteAllCommandsJsonToOutputStream(HttpResponse response)
        {
            var commands = commandFactory.GetAllHystrixCommands();

            foreach (var commandMetric in commands)
            {
                // write command metrics
                var comandJsonString = GetCommandJson(commandMetric);
                var wrappedCommandJsonString = string.IsNullOrEmpty(comandJsonString) ? "ping: \n" : "data:" + comandJsonString + "\n\n";

                await WriteStringToOutputStream(response, wrappedCommandJsonString).ConfigureAwait(false);
            }

            if (!commands.Any())
            {
                await WriteStringToOutputStream(response, "ping: \n").ConfigureAwait(false);
            }
        }

        private static async Task WriteStringToOutputStream(HttpResponse response, string wrappedJsonString)
        {
            using (var sw = new StreamWriter(response.Body, Encoding.UTF8, 1024, true))
            {
                await sw.WriteAsync(wrappedJsonString).ConfigureAwait(false);
                await sw.FlushAsync();
            }

            response.Body.Flush();
        }

        public string GetCommandJson(IHystrixCommand command)
        {
            var commandIdentifier = command.CommandIdentifier;
            var circuitBreaker = command.CircuitBreaker;
            var commandMetrics = command.CommandMetrics;
            var configurationService = commandMetrics.ConfigurationService;

            var healthCounts = commandMetrics.GetHealthCounts();

            string serializeObject = JsonConvert.SerializeObject(new
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

            return serializeObject;
        }
    }
}