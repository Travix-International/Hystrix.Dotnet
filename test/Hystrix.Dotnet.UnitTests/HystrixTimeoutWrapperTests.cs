﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixTimeoutWrapperTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixTimeoutWrapper(null, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixTimeoutWrapper(commandIdentifier, null));
            }
        }

        public class Execute
        {
            [Fact]
            public void Runs_The_Primary_Function()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                string value = timeoutWrapper.Execute(primaryFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public void Throws_HystrixTimeoutException_If_Timeout_Is_Exceeded()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(func => func()).Returns(() =>
                {
                    Thread.Sleep(5000);
                    return "a value";
                });

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5);

                // Act
                Assert.Throws<HystrixTimeoutException>(() => timeoutWrapper.Execute(primaryFunctionMock.Object));
            }

            [Fact]
            public void Throws_Original_Exception_When_Primary_Function_Throws_Exception()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(func => func()).Throws<ArgumentNullException>();

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                Assert.Throws<ArgumentNullException>(() => timeoutWrapper.Execute(primaryFunctionMock.Object));
            }
        }

        public class ExecuteAsync
        {
            [Fact]
            public async Task Runs_The_Primary_Task()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryTask = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                string value = await timeoutWrapper.ExecuteAsync(primaryTask);

                Assert.Equal("a value", value);
            }

            [Fact]
            public async Task Throws_HystrixTimeoutException_If_Timeout_Is_Exceeded()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryTask = new Func<Task<string>>(() =>  Task.Run(async () =>
                {
                    Thread.Sleep(3000);
                    await Task.Delay(10); 
                    return "a value";
                }));

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(50);

                // Act
                await Assert.ThrowsAsync<HystrixTimeoutException>(() => timeoutWrapper.ExecuteAsync(primaryTask));
            }

            [Fact]
            public async Task Throws_Original_Exception_When_Primary_Task_Throws_Exception()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "key");
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationServiceMock.Object);
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<ArgumentNullException>();

                configurationServiceMock.Setup(service => service.GetCommandTimeoutInMilliseconds()).Returns(5000);

                // Act
                await Assert.ThrowsAsync<ArgumentNullException>(() => timeoutWrapper.ExecuteAsync(primaryFunctionMock.Object));
            }
        }
    }
}