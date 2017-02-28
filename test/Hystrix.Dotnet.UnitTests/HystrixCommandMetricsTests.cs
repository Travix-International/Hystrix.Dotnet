using System;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixCommandMetricsTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandMetrics(null, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandMetrics(commandIdentifier, null));
            }
        }

        public class GetHealthCounts
        {
            [Fact]
            public void Returns_HealthCounts_With_All_Zero_Values_When_No_Requests_Have_Been_Processed()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(service => service.GetMetricsRollingStatisticalWindowInMilliseconds()).Returns(10000);
                configurationServiceMock.Setup(service => service.GetMetricsRollingStatisticalWindowBuckets()).Returns(10);
                configurationServiceMock.Setup(service => service.GetMetricsRollingPercentileWindowInMilliseconds()).Returns(10000);
                configurationServiceMock.Setup(service => service.GetMetricsRollingPercentileWindowBuckets()).Returns(10);
                configurationServiceMock.Setup(service => service.GetMetricsRollingPercentileBucketSize()).Returns(1000);
                var metricsCollector = new HystrixCommandMetrics(commandIdentifier, configurationServiceMock.Object);

                // Act
                var healthCounts = metricsCollector.GetHealthCounts();

                Assert.NotNull(healthCounts);
                Assert.Equal(0, healthCounts.GetTotalRequests());
                Assert.Equal(0, healthCounts.GetErrorCount());
                Assert.Equal(0, healthCounts.GetErrorPercentage());
            }
        }
    }
}
