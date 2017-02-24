//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;

//namespace Hystrix.Dotnet.AspNet
//{
//    internal class HystrixAppSettingsProcesser
//    {
//        private struct HystrixCommandAppSetting
//        {
//            public HystrixCommandAppSetting(string settingKey)
//            {
//                FullSettingKey = settingKey;

//                // The format of the individual settings is
//                // <add key="HystrixCommand-{GroupKey}-{CommandKey}-CommandTimeoutInMilliseconds" value="1000" />
//                var split = settingKey.Split('-');
//                GroupKey = split[1];
//                CommandKey = split[2];
//                OptionName = split[3];
//            }

//            public string FullSettingKey { get; }

//            public string GroupKey { get; }

//            public string CommandKey { get; }

//            public string OptionName { get; }
//        }

//        private int? GetSettingValueAsInt(HystrixCommandAppSetting? setting)
//        {
//            if (!setting.HasValue)
//            {
//                return null;
//            }

//            var value = ConfigurationManager.AppSettings[setting.Value.FullSettingKey];

//            int result;

//            return Int32.TryParse(value, out result) ? (int?)result : null;
//        }

//        private bool? GetSettingValueAsBool(HystrixCommandAppSetting? setting)
//        {
//            if (!setting.HasValue)
//            {
//                return null;
//            }

//            var value = ConfigurationManager.AppSettings[setting.Value.FullSettingKey];

//            bool result;

//            return Boolean.TryParse(value, out result) ? (bool?)result : null;
//        }

//        private HystrixCommandOptions AssembleOptionsFromSettings(IList<HystrixCommandAppSetting> settings)
//        {
//            var defaultOptions = HystrixCommandOptions.CreateDefault();

//            return new HystrixCommandOptions
//            {
//                CommandTimeoutInMilliseconds = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "CommandTimeoutInMilliseconds")) ?? defaultOptions.CommandTimeoutInMilliseconds,
//                CircuitBreakerForcedOpen = GetSettingValueAsBool(settings.FirstOrDefault(s => s.OptionName == "CircuitBreakerForcedOpen")) ?? defaultOptions.CircuitBreakerForcedOpen,
//                CircuitBreakerForcedClosed = GetSettingValueAsBool(settings.FirstOrDefault(s => s.OptionName == "CircuitBreakerForcedClosed")) ?? defaultOptions.CircuitBreakerForcedClosed,
//                CircuitBreakerErrorThresholdPercentage = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "CircuitBreakerErrorThresholdPercentage")) ?? defaultOptions.CircuitBreakerErrorThresholdPercentage,
//                CircuitBreakerSleepWindowInMilliseconds = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "CircuitBreakerSleepWindowInMilliseconds")) ?? defaultOptions.CircuitBreakerSleepWindowInMilliseconds,
//                CircuitBreakerRequestVolumeThreshold = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "CircuitBreakerRequestVolumeThreshold")) ?? defaultOptions.CircuitBreakerRequestVolumeThreshold,
//                MetricsHealthSnapshotIntervalInMilliseconds = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsHealthSnapshotIntervalInMilliseconds")) ?? defaultOptions.MetricsHealthSnapshotIntervalInMilliseconds,
//                MetricsRollingStatisticalWindowInMilliseconds = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingStatisticalWindowInMilliseconds")) ?? defaultOptions.MetricsRollingStatisticalWindowInMilliseconds,
//                MetricsRollingStatisticalWindowBuckets = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingStatisticalWindowBuckets")) ?? defaultOptions.MetricsRollingStatisticalWindowBuckets,
//                MetricsRollingPercentileEnabled = GetSettingValueAsBool(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingPercentileEnabled")) ?? defaultOptions.MetricsRollingPercentileEnabled,
//                MetricsRollingPercentileWindowInMilliseconds = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingPercentileWindowInMilliseconds")) ?? defaultOptions.MetricsRollingPercentileWindowInMilliseconds,
//                MetricsRollingPercentileWindowBuckets = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingPercentileWindowBuckets")) ?? defaultOptions.MetricsRollingPercentileWindowBuckets,
//                MetricsRollingPercentileBucketSize = GetSettingValueAsInt(settings.FirstOrDefault(s => s.OptionName == "MetricsRollingPercentileBucketSize")) ?? defaultOptions.MetricsRollingPercentileBucketSize,
//                HystrixCommandEnabled = GetSettingValueAsBool(settings.FirstOrDefault(s => s.OptionName == "HystrixCommandEnabled")) ?? defaultOptions.HystrixCommandEnabled
//            };
//        }

//        public HystrixOptions CreateOptionsFromAppSettings()
//        {
//            return new HystrixOptions
//            {
//                ConfigurationServiceImplementation = ConfigurationManager.AppSettings["HystrixCommandFactory-ConfigurationServiceImplementation"],
//                JsonConfigurationSourceOptions = new HystrixJsonConfigurationSourceOptions
//                {
//                    BaseLocation = ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-BaseLocation"],
//                    LocationPattern = ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-LocationPattern"],
//                    PollingIntervalInMilliseconds = Int32.Parse(ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-PollingIntervalInMilliseconds"])
//                },
//                LocalOptions = new HystrixLocalOptions
//                {
//                    // We have to transform the flat appSetting structure into an object model.
//                    CommandOptions = ConfigurationManager.AppSettings
//                        .AllKeys
//                        .Where(k => k.StartsWith("HystrixCommand"))
//                        .Select(k => new HystrixCommandAppSetting(k))
//                        .GroupBy(setting => setting.GroupKey)
//                        .ToDictionary(
//                            groupKvp => groupKvp.Key,
//                            groupKvp => groupKvp
//                                .GroupBy(setting => setting.CommandKey)
//                                .ToDictionary(
//                                    commandKvp => commandKvp.Key,
//                                    commandKvp => AssembleOptionsFromSettings(commandKvp.ToList())))
//                }
//            };
//        }
//    }
//}