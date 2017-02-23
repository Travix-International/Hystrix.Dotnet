using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JitterMagic;
using Newtonsoft.Json;

namespace Hystrix.Dotnet
{
    public class HystrixJsonConfigConfigurationService : IHystrixConfigurationService, IDisposable
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixJsonConfigConfigurationService));

        private HystrixCommandOptions configurationObject;

        private readonly int pollingIntervalInMillisecond;

        private readonly Uri configurationFileUrl;

        private readonly Uri defaultConfigurationFileUrl;

        private CancellationTokenSource cancellationTokenSource;

        public HystrixJsonConfigConfigurationService(HystrixCommandIdentifier commandIdentifier, HystrixJsonConfigurationSourceOptions options)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException(nameof(commandIdentifier));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //Log.InfoFormat("Loading web config values for group {0} and key {1}", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

            //Log.InfoFormat("PollingIntervalInMillisecond for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, pollingIntervalInMillisecond);

            //Log.InfoFormat("BaseLocation for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, baseLocation);

            //Log.InfoFormat("LocationPattern for group {0} and key {1} is {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, locationPattern);

            pollingIntervalInMillisecond = options.PollingIntervalInMilliseconds;

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
                        //Log.DebugFormat("Loading configuration from {0} in {1}ms", configurationFileUrl, interval);
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
                catch (Exception)
                {
                    //Log.Warn(string.Format("Failed loading {0}", configurationFileUrl), exception);
                }
            }).ConfigureAwait(false);
        }

        private async Task LoadRemoteConfigInternal(int timeoutInMilliseconds = 1000)
        {
            //Log.InfoFormat("Loading remote config for group {0} and key {1} from {2}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, configurationFileUrl);

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
                catch (Exception)
                {
                    //Log.WarnFormat(string.Format("Loading config from {0} for group {1} and key {2} has failed. Falling back to {3}", commandIdentifier.GroupKey, commandIdentifier.CommandKey, configurationFileUrl, defaultConfigurationFileUrl), ex);

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
                var message = string.Format("Schema {0} for base url {1} is not supported", configurationFileUrl.Scheme, configurationFileUrl);
                //Log.Error(message);
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

        //internal class ConfigurationObject
        //{
        //    public bool HystrixCommandEnabled { get; set; }

        //    public int CommandTimeoutInMilliseconds { get; set; }

        //    public bool CircuitBreakerForcedOpen { get; set; }

        //    public bool CircuitBreakerForcedClosed { get; set; }

        //    public int CircuitBreakerErrorThresholdPercentage { get; set; }

        //    public int CircuitBreakerSleepWindowInMilliseconds { get; set; }

        //    public int CircuitBreakerRequestVolumeThreshold { get; set; }

        //    public int MetricsHealthSnapshotIntervalInMilliseconds { get; set; }

        //    public int MetricsRollingStatisticalWindowInMilliseconds { get; set; }

        //    public int MetricsRollingStatisticalWindowBuckets { get; set; }

        //    public bool MetricsRollingPercentileEnabled { get; set; }

        //    public int MetricsRollingPercentileWindowInMilliseconds { get; set; }

        //    public int MetricsRollingPercentileWindowBuckets { get; set; }

        //    public int MetricsRollingPercentileBucketSize { get; set; }
        //}

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
