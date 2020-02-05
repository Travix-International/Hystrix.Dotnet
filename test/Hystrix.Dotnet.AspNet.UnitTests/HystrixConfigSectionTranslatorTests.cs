using System;
using Xunit;

namespace Hystrix.Dotnet.AspNet.UnitTests
{
    public class HystrixConfigSectionTranslatorTests
    {
        internal class TestHystrixCommandGroupCollection : HystrixCommandGroupCollection
        {
            public override bool IsReadOnly() => false;

            public void Add(HystrixCommandGroupElement element)
            {
                BaseAdd(element);
            }
        }

        internal class TestHystrixCommandCollection : HystrixCommandCollection
        {
            public override bool IsReadOnly() => false;

            public void Add(HystrixCommandElement element)
            {
                BaseAdd(element);
            }
        }

        [Fact]
        public void TranslateToOptions_SectionNull_Exception()
        {
            var sut = new HystrixConfigSectionTranslator();

            // Act
            Assert.Throws<ArgumentNullException>(() => sut.TranslateToOptions(null));
        }

        [Fact]
        public void TranslateToOptions_Called_SectionCorrectlyTranslated()
        {
            var sut = new HystrixConfigSectionTranslator();

            // Act
            var options = sut.TranslateToOptions(new HystrixConfigSection
            {
                ServiceImplementation = "TestConfigurationServiceImplementation",
                MetricsStreamPollIntervalInMilliseconds = 1000,
                JsonConfigurationSourceOptions = new HystrixJsonConfigurationSourceOptionsElement
                {
                    PollingIntervalInMilliseconds = 1001,
                    BaseLocation = "TestBaseLocation",
                    LocationPattern = "TestLocationPattern"
                },
                LocalOptions = new HystrixLocalOptionsElement
                {
                    DefaultConfiguration = new HystrixDefaultConfigurationElement
                    {
                        CommandTimeoutInMilliseconds = 1001,
                        CircuitBreakerForcedOpen = true,
                        CircuitBreakerForcedClosed = false,
                        CircuitBreakerErrorThresholdPercentage = 1002,
                        CircuitBreakerSleepWindowInMilliseconds = 1003,
                        CircuitBreakerRequestVolumeThreshold = 1004,
                        MetricsHealthSnapshotIntervalInMilliseconds = 1005,
                        MetricsRollingStatisticalWindowInMilliseconds = 1006,
                        MetricsRollingStatisticalWindowBuckets = 1007,
                        MetricsRollingPercentileEnabled = true,
                        MetricsRollingPercentileWindowInMilliseconds = 1008,
                        MetricsRollingPercentileWindowBuckets = 1009,
                        MetricsRollingPercentileBucketSize = 1010,
                        HystrixCommandEnabled = false
                    },
                    CommandGroups = new TestHystrixCommandGroupCollection
                    {
                        new HystrixCommandGroupElement
                        {
                            Key = "GroupA",
                            Commands = new TestHystrixCommandCollection
                            {
                                new HystrixCommandElement
                                {
                                    Key = "CommandA",
                                    CommandTimeoutInMilliseconds = 2001,
                                    CircuitBreakerForcedOpen = true,
                                    CircuitBreakerForcedClosed = false,
                                    CircuitBreakerErrorThresholdPercentage = 2002,
                                    CircuitBreakerSleepWindowInMilliseconds = 2003,
                                    CircuitBreakerRequestVolumeThreshold = 2004,
                                    MetricsHealthSnapshotIntervalInMilliseconds = 2005,
                                    MetricsRollingStatisticalWindowInMilliseconds = 2006,
                                    MetricsRollingStatisticalWindowBuckets = 2007,
                                    MetricsRollingPercentileEnabled = true,
                                    MetricsRollingPercentileWindowInMilliseconds = 2008,
                                    MetricsRollingPercentileWindowBuckets = 2009,
                                    MetricsRollingPercentileBucketSize = 2010,
                                    HystrixCommandEnabled = false
                                },
                                new HystrixCommandElement
                                {
                                    Key = "CommandB"
                                }
                            }
                        },
                        new HystrixCommandGroupElement
                        {
                            Key = "GroupB",
                            Commands = new TestHystrixCommandCollection
                            {
                                new HystrixCommandElement
                                {
                                    Key = "CommandC"
                                }
                            }
                        }
                    }
                }
            });

            Assert.Equal("TestConfigurationServiceImplementation", options.ConfigurationServiceImplementation);
            Assert.Equal(1000, options.MetricsStreamPollIntervalInMilliseconds);
            Assert.Equal(1001, options.JsonConfigurationSourceOptions.PollingIntervalInMilliseconds);
            Assert.Equal("TestBaseLocation", options.JsonConfigurationSourceOptions.BaseLocation);
            Assert.Equal("TestLocationPattern", options.JsonConfigurationSourceOptions.LocationPattern);
            Assert.Equal(2, options.LocalOptions.CommandGroups.Count);
            Assert.Equal(2, options.LocalOptions.CommandGroups["GroupA"].Count);
            Assert.Equal(1, options.LocalOptions.CommandGroups["GroupB"].Count);

            Assert.True(options.LocalOptions.CommandGroups["GroupA"].ContainsKey("CommandA"));

            var defaultConfig = options.LocalOptions.DefaultOptions;
            Assert.Equal(1001, defaultConfig.CommandTimeoutInMilliseconds);
            Assert.Equal(true, defaultConfig.CircuitBreakerForcedOpen);
            Assert.Equal(false, defaultConfig.CircuitBreakerForcedClosed);
            Assert.Equal(1002, defaultConfig.CircuitBreakerErrorThresholdPercentage);
            Assert.Equal(1003, defaultConfig.CircuitBreakerSleepWindowInMilliseconds);
            Assert.Equal(1004, defaultConfig.CircuitBreakerRequestVolumeThreshold);
            Assert.Equal(1005, defaultConfig.MetricsHealthSnapshotIntervalInMilliseconds);
            Assert.Equal(1006, defaultConfig.MetricsRollingStatisticalWindowInMilliseconds);
            Assert.Equal(1007, defaultConfig.MetricsRollingStatisticalWindowBuckets);
            Assert.Equal(true, defaultConfig.MetricsRollingPercentileEnabled);
            Assert.Equal(1008, defaultConfig.MetricsRollingPercentileWindowInMilliseconds);
            Assert.Equal(1009, defaultConfig.MetricsRollingPercentileWindowBuckets);
            Assert.Equal(1010, defaultConfig.MetricsRollingPercentileBucketSize);
            Assert.Equal(false, defaultConfig.HystrixCommandEnabled);

            var commandA = options.LocalOptions.CommandGroups["GroupA"]["CommandA"];
            Assert.Equal(2001, commandA.CommandTimeoutInMilliseconds);
            Assert.Equal(true, commandA.CircuitBreakerForcedOpen);
            Assert.Equal(false, commandA.CircuitBreakerForcedClosed);
            Assert.Equal(2002, commandA.CircuitBreakerErrorThresholdPercentage);
            Assert.Equal(2003, commandA.CircuitBreakerSleepWindowInMilliseconds);
            Assert.Equal(2004, commandA.CircuitBreakerRequestVolumeThreshold);
            Assert.Equal(2005, commandA.MetricsHealthSnapshotIntervalInMilliseconds);
            Assert.Equal(2006, commandA.MetricsRollingStatisticalWindowInMilliseconds);
            Assert.Equal(2007, commandA.MetricsRollingStatisticalWindowBuckets);
            Assert.Equal(true, commandA.MetricsRollingPercentileEnabled);
            Assert.Equal(2008, commandA.MetricsRollingPercentileWindowInMilliseconds);
            Assert.Equal(2009, commandA.MetricsRollingPercentileWindowBuckets);
            Assert.Equal(2010, commandA.MetricsRollingPercentileBucketSize);
            Assert.Equal(false, commandA.HystrixCommandEnabled);

            Assert.True(options.LocalOptions.CommandGroups["GroupA"].ContainsKey("CommandB"));
            Assert.True(options.LocalOptions.CommandGroups["GroupB"].ContainsKey("CommandC"));
        }
    }
}