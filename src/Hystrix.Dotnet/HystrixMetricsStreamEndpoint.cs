using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Hystrix.Dotnet
{
    internal class HystrixMetricsStreamEndpoint : IHystrixMetricsStreamEndpoint, IDisposable
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixMetricsStreamEndpoint));

        private readonly IHystrixCommandFactory commandFactory;
        private readonly int pollingInterval;
        private static readonly DateTimeProvider DateTimeProvider = new DateTimeProvider();
        private CancellationTokenSource cancellationTokenSource;

        public HystrixMetricsStreamEndpoint(IHystrixCommandFactory commandFactory, int pollingInterval)
        {
            if (commandFactory == null)
            {
                throw new ArgumentNullException("commandFactory");
            }
            if (pollingInterval < 100)
            {
                throw new ArgumentOutOfRangeException("pollingInterval", "Parameter pollingInterval needs to be greater than or equal to 100");
            }

            this.commandFactory = commandFactory;
            this.pollingInterval = pollingInterval;
        }

        public async Task PushContentToOutputStream(HttpResponseBase response)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                try
                {
                    //Log.Info("Start writing to Hystrix outputstream");

                    while (true)
                    {
                        if (token.IsCancellationRequested || response.IsClientConnected == false)
                        {
                            break;
                        }

                        await WriteAllCommandsJsonToOutputStream(response).ConfigureAwait(false);

                        await Task.Delay(pollingInterval, token).ConfigureAwait(false);
                    }
                }
                catch (HttpException)
                {
                    //Log.Error("An error occured in Hystrix outputstream", e);
                }
                finally
                {
                    //Log.Info("Flushing and closing Hystrix outputstream");

                    // Close output stream as we are done
                    response.OutputStream.Flush();
                    response.OutputStream.Close();
                    response.Flush();
                }
            }, token).ConfigureAwait(false);
        }

        public async Task WriteAllCommandsJsonToOutputStream(HttpResponseBase response)
        {
            var commands = commandFactory.GetAllHystrixCommands();

            foreach (var commandMetric in commands)
            {
                // write command metrics
                string comandJsonString = await GetCommandJson(commandMetric).ConfigureAwait(false);
                string wrappedCommandJsonString = string.IsNullOrEmpty(comandJsonString) ? "ping: \n" : "data:" + comandJsonString + "\n\n";

                await WriteStringToOutputStream(response, wrappedCommandJsonString).ConfigureAwait(false);

                // write thread pool metrics
                //string threadPoolJsonString = await GetThreadPoolJson(commandMetric).ConfigureAwait(false);
                //string wrappedThreadPoolJsonString = string.IsNullOrEmpty(threadPoolJsonString) ? "ping: \n" : "data:" + threadPoolJsonString + "\n\n";

                //await WriteStringToOutputStream(response, wrappedThreadPoolJsonString).ConfigureAwait(false);
            }

            if (!commands.Any())
            {
                await WriteStringToOutputStream(response, "ping: \n").ConfigureAwait(false);
            }
        }

        private static async Task WriteStringToOutputStream(HttpResponseBase response, string wrappedJsonString)
        {
            Stream outputStream = response.OutputStream;

            byte[] buffer = Encoding.UTF8.GetBytes(wrappedJsonString);
            await outputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            await outputStream.FlushAsync().ConfigureAwait(false);

            response.Flush();
        }

        public async Task<string> GetCommandJson(IHystrixCommand command)
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
                currentTime = DateTimeProvider.GetCurrentTimeInMilliseconds(),
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
                //threadPool = string.Empty, // commandMetrics.getThreadPoolKey().name(),
            });

            return await Task.FromResult(serializeObject).ConfigureAwait(false);
        }

        public async Task<string> GetThreadPoolJson(IHystrixCommand command)
        {
            IHystrixThreadPoolMetrics threadPoolMetrics = command.ThreadPoolMetrics;

            string serializeObject = JsonConvert.SerializeObject(new
            {
                type = "HystrixThreadPool",
                name = command.CommandIdentifier.CommandKey,

                currentTime = DateTimeProvider.GetCurrentTimeInMilliseconds(),
                currentActiveCount = threadPoolMetrics.GetCurrentActiveCount(),
                currentCompletedTaskCount = threadPoolMetrics.GetCurrentCompletedTaskCount(),
                currentCorePoolSize = threadPoolMetrics.GetCurrentCorePoolSize(),
                currentLargestPoolSize = threadPoolMetrics.GetCurrentLargestPoolSize(),
                currentMaximumPoolSize = threadPoolMetrics.GetCurrentMaximumPoolSize(),
                currentPoolSize = threadPoolMetrics.GetCurrentPoolSize(),
                currentQueueSize = threadPoolMetrics.GetCurrentQueueSize(),
                currentTaskCount = threadPoolMetrics.GetCurrentTaskCount(),
                rollingCountThreadsExecuted = threadPoolMetrics.GetRollingCountThreadsExecuted(),
                rollingMaxActiveThreads = threadPoolMetrics.GetRollingMaxActiveThreads(),
                rollingCountCommandRejections = threadPoolMetrics.GetRollingCountThreadPoolRejected(),

                propertyValue_queueSizeRejectionThreshold = 0, //threadPoolMetrics.ConfigurationService.queueSizeRejectionThreshold().get(),
                propertyValue_metricsRollingStatisticalWindowInMilliseconds = threadPoolMetrics.ConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds(),

                reportingHosts = 1
            });

            return await Task.FromResult(serializeObject).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (cancellationTokenSource != null)
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }

                cancellationTokenSource.Dispose();
            }
        }
    }
}