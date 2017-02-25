using System;
using System.Linq;

namespace Hystrix.Dotnet.AspNet
{
    internal class HystrixConfigSectionTranslator
    {
        public HystrixOptions TranslateToOptions(HystrixConfigSection configSection)
        {
            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            return new HystrixOptions
            {
                ConfigurationServiceImplementation = configSection.ServiceImplementation,
                MetricsStreamPollIntervalInMilliseconds = configSection.MetricsStreamPollIntervalInMilliseconds,
                JsonConfigurationSourceOptions = TranslateToJsonConfigurationSourceOptions(configSection.JsonConfigurationSourceOptions),
                LocalOptions = TranslateToLocalOptions(configSection.LocalOptions)
            };
        }

        private HystrixJsonConfigurationSourceOptions TranslateToJsonConfigurationSourceOptions(HystrixJsonConfigurationSourceOptionsElement element)
        {
            return element == null
                ? null
                : new HystrixJsonConfigurationSourceOptions
                {
                    BaseLocation = element.BaseLocation,
                    LocationPattern = element.LocationPattern,
                    PollingIntervalInMilliseconds = element.PollingIntervalInMilliseconds
                };
        }

        private HystrixLocalOptions TranslateToLocalOptions(HystrixLocalOptionsElement element)
        {
            return new HystrixLocalOptions
            {
                CommandOptions = element.CommandGroups
                    .Cast<HystrixCommandGroupElement>()
                    .ToDictionary(
                        commandGroup => commandGroup.Key,
                        commandGroup => commandGroup.Commands
                            .Cast<HystrixCommandElement>()
                            .ToDictionary(
                                command => command.Key,
                                TranslateToCommandOptions))
            };
        }

        private HystrixCommandOptions TranslateToCommandOptions(HystrixCommandElement command)
        {
            return new HystrixCommandOptions
            {
                CommandTimeoutInMilliseconds = command.CommandTimeoutInMilliseconds,
                CircuitBreakerForcedOpen = command.CircuitBreakerForcedOpen,
                CircuitBreakerForcedClosed = command.CircuitBreakerForcedClosed,
                CircuitBreakerErrorThresholdPercentage = command.CircuitBreakerErrorThresholdPercentage,
                CircuitBreakerSleepWindowInMilliseconds = command.CircuitBreakerSleepWindowInMilliseconds,
                CircuitBreakerRequestVolumeThreshold = command.CircuitBreakerRequestVolumeThreshold,
                MetricsHealthSnapshotIntervalInMilliseconds = command.MetricsHealthSnapshotIntervalInMilliseconds,
                MetricsRollingStatisticalWindowInMilliseconds = command.MetricsRollingStatisticalWindowInMilliseconds,
                MetricsRollingStatisticalWindowBuckets = command.MetricsRollingStatisticalWindowBuckets,
                MetricsRollingPercentileEnabled = command.MetricsRollingPercentileEnabled,
                MetricsRollingPercentileWindowInMilliseconds = command.MetricsRollingPercentileWindowInMilliseconds,
                MetricsRollingPercentileWindowBuckets = command.MetricsRollingPercentileWindowBuckets,
                MetricsRollingPercentileBucketSize = command.MetricsRollingPercentileBucketSize,
                HystrixCommandEnabled = command.HystrixCommandEnabled,
            };
        }
    }
}