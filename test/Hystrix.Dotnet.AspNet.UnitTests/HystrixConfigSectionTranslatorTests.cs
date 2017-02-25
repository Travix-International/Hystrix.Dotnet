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
            Assert.Equal(2, options.LocalOptions.CommandOptions.Count);
            Assert.Equal(2, options.LocalOptions.CommandOptions["GroupA"].Count);
            Assert.Equal(1, options.LocalOptions.CommandOptions["GroupB"].Count);

            Assert.True(options.LocalOptions.CommandOptions["GroupA"].ContainsKey("CommandA"));

            var commandA = options.LocalOptions.CommandOptions["GroupA"]["CommandA"];
            Assert.Equal(1001, commandA.CommandTimeoutInMilliseconds);
            Assert.Equal(true, commandA.CircuitBreakerForcedOpen);
            Assert.Equal(false, commandA.CircuitBreakerForcedClosed);
            Assert.Equal(1002, commandA.CircuitBreakerErrorThresholdPercentage);
            Assert.Equal(1003, commandA.CircuitBreakerSleepWindowInMilliseconds);
            Assert.Equal(1004, commandA.CircuitBreakerRequestVolumeThreshold);
            Assert.Equal(1005, commandA.MetricsHealthSnapshotIntervalInMilliseconds);
            Assert.Equal(1006, commandA.MetricsRollingStatisticalWindowInMilliseconds);
            Assert.Equal(1007, commandA.MetricsRollingStatisticalWindowBuckets);
            Assert.Equal(true, commandA.MetricsRollingPercentileEnabled);
            Assert.Equal(1008, commandA.MetricsRollingPercentileWindowInMilliseconds);
            Assert.Equal(1009, commandA.MetricsRollingPercentileWindowBuckets);
            Assert.Equal(1010, commandA.MetricsRollingPercentileBucketSize);
            Assert.Equal(false, commandA.HystrixCommandEnabled);

            Assert.True(options.LocalOptions.CommandOptions["GroupA"].ContainsKey("CommandB"));
            Assert.True(options.LocalOptions.CommandOptions["GroupB"].ContainsKey("CommandC"));
        }
    }
}