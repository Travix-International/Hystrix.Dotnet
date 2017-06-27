using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JitterMagic;
using Hystrix.Dotnet.Logging;
using Newtonsoft.Json;

namespace Hystrix.Dotnet
{
    public class HystrixJsonConfigConfigurationService : IHystrixConfigurationService, IDisposable
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixJsonConfigConfigurationService));

        private readonly HystrixCommandIdentifier commandIdentifier;

        private readonly int pollingIntervalInMillisecond;

        private readonly Uri configurationFileUrl;

        private readonly Uri defaultConfigurationFileUrl;

        private HystrixCommandOptions configurationObject;

        private CancellationTokenSource cancellationTokenSource;

        public HystrixJsonConfigConfigurationService(HystrixCommandIdentifier commandIdentifier, HystrixJsonConfigurationSourceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.commandIdentifier = commandIdentifier ?? throw new ArgumentNullException(nameof(commandIdentifier));

            pollingIntervalInMillisecond = options.PollingIntervalInMilliseconds;

            log.InfoFormat("Loading web config values for group {0} and key {1}", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

            log.InfoFormat("PollingIntervalInMillisecond for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, pollingIntervalInMillisecond);

            log.InfoFormat("BaseLocation for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, options.BaseLocation);

            log.InfoFormat("LocationPattern for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, options.LocationPattern);

            Uri baseLocationUrl;
            if (!Uri.TryCreate(EnsureTrailingSlash(options.BaseLocation), UriKind.Absolute, out baseLocationUrl))
            {
                throw new ConfigurationException("Options.BaseLocation has to contain a valid url.");
            }

            if (!Uri.TryCreate(baseLocationUrl, string.Format(options.LocationPattern, commandIdentifier.GroupKey, commandIdentifier.CommandKey), out configurationFileUrl))
            {
                throw new ConfigurationException("Options.BaseLocation has to contain a valid url.");
            }

            if (!Uri.TryCreate(baseLocationUrl, "Default.json", out defaultConfigurationFileUrl))
            {
                throw new ConfigurationException("Options.BaseLocation has to contain a valid url.");
            }

            // load remote config synchronous first time (might throw an aggregate exception)
            LoadRemoteConfigInternal(60000).Wait();

            LoadRemoteConfigAfterInterval();
        }

        private static string EnsureTrailingSlash(string str) => str.EndsWith("/") || str.EndsWith(@"\") ? str : str + "/";

        /// <summary>
        /// Run background polling for remote config changes
        /// </summary>
        private void LoadRemoteConfigAfterInterval()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        // wait for an interval with jitter
                        var interval = Jitter.Apply(pollingIntervalInMillisecond);
                        log.DebugFormat("Loading configuration from {0} in {1}ms", configurationFileUrl, interval);
                        await Task.Delay(interval, token).ConfigureAwait(false);

                        await LoadRemoteConfig().ConfigureAwait(false);
                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Loads configuration from url returning json file in a background task
        /// </summary>
        private async Task LoadRemoteConfig()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await LoadRemoteConfigInternal().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.Warn($"Failed loading {configurationFileUrl}", ex);
                }
            }).ConfigureAwait(false);
        }

        private async Task LoadRemoteConfigInternal(int timeoutInMilliseconds = 1000)
        {
            log.InfoFormat("Loading remote config for group {0} and key {1} from {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, configurationFileUrl);

            #if !COREFX
            if (configurationFileUrl.Scheme == Uri.UriSchemeFile)
            #else
            if (configurationFileUrl.Scheme == "file")
            #endif
            {
                using (var reader = File.OpenText(configurationFileUrl.LocalPath))
                {
                    configurationObject = DeserializeResponse(await reader.ReadToEndAsync().ConfigureAwait(false));
                }
            }
            #if !COREFX
            else if (configurationFileUrl.Scheme == Uri.UriSchemeHttp || configurationFileUrl.Scheme == Uri.UriSchemeHttps)
            #else
            else if (configurationFileUrl.Scheme == "http" || configurationFileUrl.Scheme == "https")
            #endif
            {
                bool fallbackToDefault = false;

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
                        configurationObject = DeserializeResponse(await httpClient.GetStringAsync(configurationFileUrl).ConfigureAwait(false));
                    }
                }
                catch (Exception ex)
                {
                    log.WarnFormat($"Loading config from {commandIdentifier.GroupKey} for group {commandIdentifier.CommandKey} and key {configurationFileUrl} has failed. Falling back to {defaultConfigurationFileUrl}", ex);

                    fallbackToDefault = true;
                }

                if (fallbackToDefault)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
                        configurationObject = DeserializeResponse(await httpClient.GetStringAsync(defaultConfigurationFileUrl).ConfigureAwait(false));
                    }
                }
            }
            else
            {
                var message = $"Schema {configurationFileUrl.Scheme} for base url {configurationFileUrl} is not supported";
                log.Error(message);
                throw new NotSupportedException(message);
            }
        }

        private HystrixCommandOptions DeserializeResponse(string json)
        {            
            return JsonConvert.DeserializeObject<HystrixCommandOptions>(json);
        }

        /// <inheritdoc/>
        public int GetCommandTimeoutInMilliseconds()
        {
            return configurationObject != null ? configurationObject.CommandTimeoutInMilliseconds : 1000;
        }

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedOpen()
        {
            return configurationObject != null && configurationObject.CircuitBreakerForcedOpen;
        }

        /// <inheritdoc/>
        public bool GetCircuitBreakerForcedClosed()
        {
            return configurationObject != null && configurationObject.CircuitBreakerForcedClosed;
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerErrorThresholdPercentage()
        {
            return configurationObject != null ? configurationObject.CircuitBreakerErrorThresholdPercentage : 50;
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerSleepWindowInMilliseconds()
        {
            return configurationObject != null ? configurationObject.CircuitBreakerSleepWindowInMilliseconds : 5000;
        }

        /// <inheritdoc/>
        public int GetCircuitBreakerRequestVolumeThreshold()
        {
            return configurationObject != null ? configurationObject.CircuitBreakerRequestVolumeThreshold : 20;
        }

        /// <inheritdoc/>
        public int GetMetricsHealthSnapshotIntervalInMilliseconds()
        {
            return configurationObject != null ? configurationObject.MetricsHealthSnapshotIntervalInMilliseconds : 500;
        }

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowInMilliseconds()
        {
            return configurationObject != null ? configurationObject.MetricsRollingStatisticalWindowInMilliseconds : 10000;
        }

        /// <inheritdoc/>
        public int GetMetricsRollingStatisticalWindowBuckets()
        {
            return configurationObject != null ? configurationObject.MetricsRollingStatisticalWindowBuckets : 10;
        }

        /// <inheritdoc/>
        public bool GetMetricsRollingPercentileEnabled()
        {
            return configurationObject == null || configurationObject.MetricsRollingPercentileEnabled;
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowInMilliseconds()
        {
            return configurationObject != null ? configurationObject.MetricsRollingPercentileWindowInMilliseconds : 60000;
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileWindowBuckets()
        {
            return configurationObject != null ? configurationObject.MetricsRollingPercentileWindowBuckets : 6;
        }

        /// <inheritdoc/>
        public int GetMetricsRollingPercentileBucketSize()
        {
            return configurationObject != null ? configurationObject.MetricsRollingPercentileBucketSize : 100;
        }

        public bool GetHystrixCommandEnabled()
        {
            return configurationObject == null || configurationObject.HystrixCommandEnabled;
        }

        internal class SingleSetting 
        {
            public string Name { get; set; }

            public string Value { get; set; }
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