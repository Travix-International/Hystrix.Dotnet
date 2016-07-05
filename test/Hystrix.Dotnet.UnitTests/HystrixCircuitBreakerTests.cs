using System;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixCircuitBreakerTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixCircuitBreaker(null, configurationServiceMock.Object, commandMetricsMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();

                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixCircuitBreaker(commandIdentifier, null, commandMetricsMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_MetricsCollector_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, null));
            }
        }

        public class AllowRequest
        {
            [Fact]
            public void Returns_False_When_GetCircuitBreakerForcedOpen_Is_True()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                configurationServiceMock.Setup(service => service.GetCircuitBreakerForcedOpen()).Returns(true);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.False(allowRequest);
            }

            [Fact]
            public void Returns_True_When_GetCircuitBreakerForcedClosed_Is_True()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                configurationServiceMock.Setup(service => service.GetCircuitBreakerForcedClosed()).Returns(true);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.True(allowRequest);
            }

            [Fact]
            public void Returns_True_When_TotalRequests_Is_Less_Than_GetCircuitBreakerRequestVolumeThreshold()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                commandMetricsMock.Setup(collector => collector.GetHealthCounts()).Returns(new HystrixHealthCounts(99, 16, 16));
                configurationServiceMock.Setup(service => service.GetCircuitBreakerRequestVolumeThreshold()).Returns(100);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.True(allowRequest);
            }

            [Fact]
            public void Returns_True_When_ErrorPercentage_Is_Less_Than_GetCircuitBreakerErrorThresholdPercentage()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                commandMetricsMock.Setup(collector => collector.GetHealthCounts()).Returns(new HystrixHealthCounts(100, 16, 16));
                configurationServiceMock.Setup(service => service.GetCircuitBreakerErrorThresholdPercentage()).Returns(17);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.True(allowRequest);
            }

            [Fact]
            public void Opens_Circuit_When_ErrorPercentage_Is_Less_Than_GetCircuitBreakerErrorThresholdPercentage()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                commandMetricsMock.Setup(collector => collector.GetHealthCounts()).Returns(new HystrixHealthCounts(100, 16, 16));
                configurationServiceMock.Setup(service => service.GetCircuitBreakerErrorThresholdPercentage()).Returns(16);

                Assert.False(circuitBreaker.CircuitIsOpen);

                // act
                circuitBreaker.AllowRequest();

                Assert.True(circuitBreaker.CircuitIsOpen);
            }

            [Fact]
            public void Returns_False_When_ErrorPercentage_Is_Less_Than_GetCircuitBreakerErrorThresholdPercentage()
            {
                var dateTimeProviderMock = new Mock<DateTimeProvider>();
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(dateTimeProviderMock.Object, commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                commandMetricsMock.Setup(collector => collector.GetHealthCounts()).Returns(new HystrixHealthCounts(100, 16, 16));
                configurationServiceMock.Setup(service => service.GetCircuitBreakerErrorThresholdPercentage()).Returns(16);
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(0);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.False(allowRequest);
            }

            [Fact]
            public void Returns_False_When_Circuit_Is_Open()
            {
                var dateTimeProviderMock = new Mock<DateTimeProvider>();
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(dateTimeProviderMock.Object, commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(0);
                circuitBreaker.OpenCircuit();

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.False(allowRequest);
            }

            [Fact]
            public void Returns_True_When_Circuit_Is_Open_But_Request_Is_First_After_GetCircuitBreakerSleepWindowInMilliseconds()
            {
                var dateTimeProviderMock = new Mock<DateTimeProvider>();
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(dateTimeProviderMock.Object, commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(0);
                circuitBreaker.OpenCircuit();

                configurationServiceMock.Setup(service => service.GetCircuitBreakerSleepWindowInMilliseconds()).Returns(5000);
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(6000);

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.True(allowRequest);
            }

            [Fact]
            public void Returns_False_When_Circuit_Is_Open_But_Request_Is_First_After_GetCircuitBreakerSleepWindowInMilliseconds()
            {
                var dateTimeProviderMock = new Mock<DateTimeProvider>();
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(dateTimeProviderMock.Object, commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(0);
                circuitBreaker.OpenCircuit();

                configurationServiceMock.Setup(service => service.GetCircuitBreakerSleepWindowInMilliseconds()).Returns(5000);
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(6000);

                // first request
                circuitBreaker.AllowRequest();

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.False(allowRequest);
            }

            [Fact]
            public void Returns_True_When_Circuit_Has_Been_Closed_After_Being_Open()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                commandMetricsMock.Setup(collector => collector.GetHealthCounts()).Returns(new HystrixHealthCounts(100, 16, 16));
                configurationServiceMock.Setup(service => service.GetCircuitBreakerErrorThresholdPercentage()).Returns(17);

                circuitBreaker.OpenCircuit();
                circuitBreaker.CloseCircuit();

                // act
                bool allowRequest = circuitBreaker.AllowRequest();

                Assert.True(allowRequest);
            }
        }

        public class OpenCircuit
        {
            [Fact]
            public void Opens_The_Circuit_When_Closed()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                Assert.False(circuitBreaker.CircuitIsOpen);

                // act
                circuitBreaker.OpenCircuit();

                Assert.True(circuitBreaker.CircuitIsOpen);
            }

            [Fact]
            public void Keeps_The_Circuit_Open_When_Open()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                circuitBreaker.OpenCircuit();
                Assert.True(circuitBreaker.CircuitIsOpen);

                // act
                circuitBreaker.OpenCircuit();

                Assert.True(circuitBreaker.CircuitIsOpen);
            }
        }


        public class CloseCircuit
        {
            [Fact]
            public void Closes_The_Circuit_When_Open()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                circuitBreaker.OpenCircuit();
                Assert.True(circuitBreaker.CircuitIsOpen);

                // act
                circuitBreaker.CloseCircuit();

                Assert.False(circuitBreaker.CircuitIsOpen);
            }

            [Fact]
            public void Keeps_The_Circuit_Closed_When_Closed()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationServiceMock.Object, commandMetricsMock.Object);

                circuitBreaker.OpenCircuit();
                Assert.True(circuitBreaker.CircuitIsOpen);
                circuitBreaker.CloseCircuit();
                Assert.False(circuitBreaker.CircuitIsOpen);

                // act
                circuitBreaker.CloseCircuit();

                Assert.False(circuitBreaker.CircuitIsOpen);
            }
        }
    }
}
