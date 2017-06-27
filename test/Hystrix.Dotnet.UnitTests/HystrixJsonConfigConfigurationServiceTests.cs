using System;
using Stubbery;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixJsonConfigConfigurationServiceTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixJsonConfigConfigurationService(null, new HystrixJsonConfigurationSourceOptions()));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_Options_Is_Null()
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixJsonConfigConfigurationService(new HystrixCommandIdentifier("a", "b"), null));
            }
        }
    }

    public class GetCommandTimeoutInMilliseconds
    {
        [Fact]
        public void Returns_Remote_Config_Value_From_File_Scheme()
        {
            var options = new HystrixJsonConfigurationSourceOptions
            {
                BaseLocation = GetLocalBaseLocation(),
                LocationPattern = "{0}-{1}.json"
            };

            var commandIdentifier = new HystrixCommandIdentifier("Group", "Command");
            var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier, options);

            // Act
            var value = configurationService.GetCommandTimeoutInMilliseconds();

            Assert.Equal(25000, value);
        }

        [Fact]
        public void Returns_Remote_Config_Value_From_Http_Scheme_Json_File()
        {
            using (var apiStub = new ApiStub())
            {
                apiStub.Get(
                    "/Group-Command.json",
                    (req, args) => @"{
""HystrixCommandEnabled"": true,
""CommandTimeoutInMilliseconds"":12345,
""CircuitBreakerForcedOpen"":false,
""CircuitBreakerForcedClosed"":false,
""CircuitBreakerErrorThresholdPercentage"":50,
""CircuitBreakerSleepWindowInMilliseconds"":5000,
""CircuitBreakerRequestVolumeThreshold"":20,
""MetricsHealthSnapshotIntervalInMilliseconds"":500,
""MetricsRollingStatisticalWindowInMilliseconds"":10000,
""MetricsRollingStatisticalWindowBuckets"":10,
""MetricsRollingPercentileEnabled"":true,
""MetricsRollingPercentileWindowInMilliseconds"":60000,
""MetricsRollingPercentileWindowBuckets"":6,
""MetricsRollingPercentileBucketSize"":100
}");

                apiStub.Start();

                var options = new HystrixJsonConfigurationSourceOptions
                {
                    BaseLocation = apiStub.Address,
                    LocationPattern = "{0}-{1}.json"
                };

                var commandIdentifier = new HystrixCommandIdentifier("Group", "Command");
                var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier, options);

                // Act
                var value = configurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(12345, value);
            }
        }

        [Fact]
        public void Returns_Values_From_Default_Json_If_Specific_Json_Is_Invalid()
        {
            using (var apiStub = new ApiStub())
            {
                apiStub.Get(
                    "/Group-Command.json",
                    (req, args) => @"This is { not a valid json """);

                apiStub.Get(
                    "/Default.json",
                    (req, args) => @"{
""HystrixCommandEnabled"": true,
""CommandTimeoutInMilliseconds"":50000,
""CircuitBreakerForcedOpen"":false,
""CircuitBreakerForcedClosed"":false,
""CircuitBreakerErrorThresholdPercentage"":50,
""CircuitBreakerSleepWindowInMilliseconds"":5000,
""CircuitBreakerRequestVolumeThreshold"":20,
""MetricsHealthSnapshotIntervalInMilliseconds"":500,
""MetricsRollingStatisticalWindowInMilliseconds"":10000,
""MetricsRollingStatisticalWindowBuckets"":10,
""MetricsRollingPercentileEnabled"":true,
""MetricsRollingPercentileWindowInMilliseconds"":60000,
""MetricsRollingPercentileWindowBuckets"":6,
""MetricsRollingPercentileBucketSize"":100
}");

                apiStub.Start();

                var options = new HystrixJsonConfigurationSourceOptions
                {
                    BaseLocation = apiStub.Address,
                    LocationPattern = "{0}-{1}.json"
                };

                var commandIdentifier = new HystrixCommandIdentifier("Group", "Command");
                var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier, options);

                // Act
                var value = configurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(50000, value);
            }
        }

        [Fact]
        public void Returns_Values_From_Default_Json_If_Specific_Json_Is_Not_Present()
        {
            using (var apiStub = new ApiStub())
            {
                apiStub.Start();

                apiStub.Get(
                    "/Default.json",
                    (req, args) => @"{
""HystrixCommandEnabled"": true,
""CommandTimeoutInMilliseconds"":50000,
""CircuitBreakerForcedOpen"":false,
""CircuitBreakerForcedClosed"":false,
""CircuitBreakerErrorThresholdPercentage"":50,
""CircuitBreakerSleepWindowInMilliseconds"":5000,
""CircuitBreakerRequestVolumeThreshold"":20,
""MetricsHealthSnapshotIntervalInMilliseconds"":500,
""MetricsRollingStatisticalWindowInMilliseconds"":10000,
""MetricsRollingStatisticalWindowBuckets"":10,
""MetricsRollingPercentileEnabled"":true,
""MetricsRollingPercentileWindowInMilliseconds"":60000,
""MetricsRollingPercentileWindowBuckets"":6,
""MetricsRollingPercentileBucketSize"":100
}");

                var options = new HystrixJsonConfigurationSourceOptions
                {
                    BaseLocation = apiStub.Address,
                    LocationPattern = "{0}-{1}.json"
                };

                var commandIdentifier = new HystrixCommandIdentifier("NoExisting", "Command");
                var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier, options);

                // Act
                var value = configurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(50000, value);
            }
        }

        private static string GetLocalBaseLocation()
        {
            return new Uri(AppContext.BaseDirectory).AbsoluteUri;
        }
    }
}