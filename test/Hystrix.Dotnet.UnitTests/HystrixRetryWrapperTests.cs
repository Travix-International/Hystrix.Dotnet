using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixRetryWrapperTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixRetryWrapper(null, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixRetryWrapper(commandIdentifier, null));
            }
        }

        public class Execute
        {
            [Fact]
            public void Runs_The_Primary_Function()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                string value = sut.Execute(primaryFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public void Throws_Retries_If_Exceptions_Is_Thrown()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.SetupSequence(func => func())
                    .Throws(new InvalidOperationException())
                    .Throws(new InvalidOperationException())
                    .Returns("a value");

                configurationServiceMock.Setup(service => service.GetCommandRetryCount()).Returns(2);

                // Act
                string value = sut.Execute(primaryFunctionMock.Object);

                primaryFunctionMock.Verify(f => f(), Times.Exactly(3));
                Assert.Equal("a value", value);
            }

            [Fact]
            public void Throws_Original_Exception_If_Retried_Too_Many_Times()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<string>>();

                primaryFunctionMock.SetupSequence(func => func())
                    .Throws(new InvalidOperationException())
                    .Throws(new InvalidOperationException())
                    .Throws(new InvalidOperationException())
                    .Returns("a");

                configurationServiceMock.Setup(service => service.GetCommandRetryCount()).Returns(2);

                // Act
                Assert.Throws<InvalidOperationException>(() => sut.Execute(primaryFunctionMock.Object));
                
                primaryFunctionMock.Verify(f => f(), Times.Exactly(3));
            }
        }

        public class ExecuteAsync
        {
            [Fact]
            public async Task Runs_The_Primary_Function()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                string value = await sut.ExecuteAsync(primaryFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public async Task Throws_Retries_If_Exceptions_Is_Thrown()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.SetupSequence(func => func())
                    .ThrowsAsync(new InvalidOperationException())
                    .ThrowsAsync(new InvalidOperationException())
                    .ReturnsAsync("a value");

                configurationServiceMock.Setup(service => service.GetCommandRetryCount()).Returns(2);

                // Act
                string value = await sut.ExecuteAsync(primaryFunctionMock.Object);

                primaryFunctionMock.Verify(f => f(), Times.Exactly(3));
                Assert.Equal("a value", value);
            }

            [Fact]
            public async Task Throws_Original_Exception_If_Retried_Too_Many_Times()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var sut = new HystrixRetryWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<Task<string>>>();

                primaryFunctionMock.SetupSequence(func => func())
                    .ThrowsAsync(new InvalidOperationException())
                    .ThrowsAsync(new InvalidOperationException())
                    .ThrowsAsync(new InvalidOperationException())
                    .ReturnsAsync("a");

                configurationServiceMock.Setup(service => service.GetCommandRetryCount()).Returns(2);

                // Act
                await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(primaryFunctionMock.Object));
                
                primaryFunctionMock.Verify(f => f(), Times.Exactly(3));
            }
        }
    }
}