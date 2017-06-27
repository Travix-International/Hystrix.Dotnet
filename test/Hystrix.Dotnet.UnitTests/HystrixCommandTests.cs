using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
// ReSharper disable MethodSupportsCancellation

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixCommandTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                var timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                var circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(null, timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_TimeoutWrapper_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "command");
                var circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(commandIdentifier, null, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_CircuitBreaker_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "command");
                var timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(commandIdentifier, timeoutWrapperMock.Object, null, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_CommandMetrics_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "command");
                var timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                var circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                var threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(commandIdentifier, timeoutWrapperMock.Object, circuitBreakerMock.Object, null, threadPoolMetricsMock.Object, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ThreadPoolMetrics_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "command");
                var timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                var circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(commandIdentifier, timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, null, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var commandIdentifier = new HystrixCommandIdentifier("group", "command");
                var timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                var circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                var commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                var threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommand(commandIdentifier, timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, null));
            }
        }

        public class Execute
        {
            private readonly Mock<IHystrixTimeoutWrapper> timeoutWrapperMock;
            private readonly Mock<IHystrixCircuitBreaker> circuitBreakerMock;
            private readonly Mock<IHystrixCommandMetrics> commandMetricsMock;
            private readonly Mock<IHystrixConfigurationService> configurationServiceMock;
            private readonly Mock<IHystrixThreadPoolMetrics> threadPoolMetricsMock;
            private readonly HystrixCommand hystrixCommand;
            
            public Execute()
            {
                timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                configurationServiceMock = new Mock<IHystrixConfigurationService>();

                hystrixCommand = new HystrixCommand(new HystrixCommandIdentifier("group", "command"), timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object);                
            }

            [Fact]
            public void Runs_Primary_Function_Directly_When_HystrixCommandEnabled_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(false);

                // Act
                string value = hystrixCommand.Execute(primaryFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public void Runs_The_Primary_Function_In_The_TimeoutWrapper_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Returns((Func<string> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                string value = hystrixCommand.Execute(primaryFunction);

                Assert.Equal("a value", value);
                timeoutWrapperMock.VerifyAll();
            }

            [Fact]
            public void Throws_HystrixCommandException_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));
            }

            [Fact]
            public void Throws_HystrixCommandException_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();

                // Act
                var hystrixCommandException = Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                Assert.IsType<HystrixTimeoutException>(hystrixCommandException.InnerExceptions[0]);
            }

            [Fact]
            public void Throws_HystrixCommandException_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                var hystrixCommandException = Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                Assert.IsType<Exception>(hystrixCommandException.InnerExceptions[0]);
            }

            [Fact]
            public void Calls_IncrementConcurrentExecutionCount_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.IncrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadExecution_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadExecution());

                // Act
                hystrixCommand.Execute(primaryFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Opens_CircuitBreaker_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                circuitBreakerMock.Setup(x => x.CloseCircuit());

                // Act
                hystrixCommand.Execute(primaryFunction);

                circuitBreakerMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddCommandExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddCommandExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkSuccess_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.MarkSuccess());

                // Act
                hystrixCommand.Execute(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkTimeout_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.MarkTimeout());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkFailure_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkFailure());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkExceptionThrown_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkExceptionThrown());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }
            
            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                hystrixCommand.Execute(primaryFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkShortCircuited_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkShortCircuited());

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                commandMetricsMock.VerifyAll();
            }
            
            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                Assert.Throws<HystrixCommandException>(() => hystrixCommand.Execute(primaryFunction));

                commandMetricsMock.VerifyAll();
            }
        }

        public class ExecuteWithFallback
        {
            private readonly Mock<IHystrixTimeoutWrapper> timeoutWrapperMock;
            private readonly Mock<IHystrixCircuitBreaker> circuitBreakerMock;
            private readonly Mock<IHystrixCommandMetrics> commandMetricsMock;
            private readonly Mock<IHystrixThreadPoolMetrics> threadPoolMetricsMock;
            private readonly Mock<IHystrixConfigurationService> configurationServiceMock;
            private readonly HystrixCommand hystrixCommand;
            
            public ExecuteWithFallback()
            {
                timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                configurationServiceMock = new Mock<IHystrixConfigurationService>();

                hystrixCommand = new HystrixCommand(new HystrixCommandIdentifier("group", "command"), timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object);
            }

            [Fact]
            public void Runs_Primary_Function_Directly_When_HystrixCommandEnabled_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(false);

                // Act
                string value = hystrixCommand.Execute(primaryFunction, fallbackFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public void Runs_The_Primary_Function_In_The_TimeoutWrapper_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Returns((Func<string> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                string value = hystrixCommand.Execute(primaryFunction, fallbackFunction);

                Assert.Equal("a value", value);
                timeoutWrapperMock.VerifyAll();
            }

            [Fact]
            public void Runs_Fallback_Function_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);

                // Act
                string value = hystrixCommand.Execute(primaryFunction, fallbackFunction);
                
                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public void Runs_Fallback_Function_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();

                // Act
                string value = hystrixCommand.Execute(primaryFunction, fallbackFunction);
                
                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public void Runs_Fallback_Function_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());

                // Act
                string value = hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public void Calls_IncrementConcurrentExecutionCount_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.IncrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadExecution_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadExecution());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Opens_CircuitBreaker_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                circuitBreakerMock.Setup(x => x.CloseCircuit());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                circuitBreakerMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddCommandExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddCommandExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkSuccess_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.MarkSuccess());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkTimeout_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.MarkTimeout());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkFailure_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkFailure());

                // Act
                hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkExceptionThrown_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkExceptionThrown());

                // Act
                hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_DecrementConcurrentExecutionCount_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkThreadCompletion_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkShortCircuited_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkShortCircuited());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<string>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.Execute(primaryFunctionMock.Object, null)).Returns((Func<string> primary) => primary());
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Calls_MarkFallbackSuccess_When_Fallback_Function_Succeeds()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkFallbackSuccess());

                // Act
                hystrixCommand.Execute(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public void Does_Not_CallsMarkFallbackSuccess_When_Fallback_Function_Throws_Any_Exception()
            {
                var primaryFunction = new Func<string>(() => "a value");
                var fallbackFunction = new Func<string>(() => "an alternative value");
                var fallbackFunctionMock = new Mock<Func<string>>();
                fallbackFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                // http://stackoverflow.com/questions/537308/how-to-verify-that-method-was-not-called-in-moq
                commandMetricsMock.Setup(x => x.MarkFallbackSuccess()).Throws(new Exception("Shouldn't be called."));

                // Act
                Assert.Throws<Exception>(() => hystrixCommand.Execute(primaryFunction, fallbackFunction));
            }
        }
        
        public class ExecuteAsync
        {
            private readonly Mock<IHystrixTimeoutWrapper> timeoutWrapperMock;
            private readonly Mock<IHystrixCircuitBreaker> circuitBreakerMock;
            private readonly Mock<IHystrixCommandMetrics> commandMetricsMock;
            private readonly Mock<IHystrixThreadPoolMetrics> threadPoolMetricsMock;
            private readonly Mock<IHystrixConfigurationService> configurationServiceMock;
            private readonly HystrixCommand hystrixCommand;
            
            public ExecuteAsync()
            {
                timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                configurationServiceMock = new Mock<IHystrixConfigurationService>();

                hystrixCommand = new HystrixCommand(new HystrixCommandIdentifier("group", "command"), timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object);
            }

            [Fact]
            public async Task Runs_Primary_Function_Directly_When_HystrixCommandEnabled_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(false);

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public async Task Runs_The_Primary_Function_In_The_TimeoutWrapper_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Returns((Func<Task<string>> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction);

                Assert.Equal("a value", value);
                timeoutWrapperMock.VerifyAll();
            }

            [Fact]
            public async Task Throws_HystrixCommandException_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));
            }

            [Fact]
            public async Task Throws_HystrixCommandException_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();

                // Act
                var hystrixCommandException = await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                Assert.IsType<HystrixTimeoutException>(hystrixCommandException.InnerExceptions[0]);
            }

            [Fact]
            public async Task Throws_HystrixCommandException_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                var hystrixCommandException = await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                Assert.IsType<Exception>(hystrixCommandException.InnerExceptions[0]);
            }

            [Fact]
            public async Task Calls_IncrementConcurrentExecutionCount_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.IncrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadExecution_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadExecution());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Opens_CircuitBreaker_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                circuitBreakerMock.Setup(x => x.CloseCircuit());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                circuitBreakerMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddCommandExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddCommandExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkSuccess_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.MarkSuccess());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkTimeout_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.MarkTimeout());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkFailure_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkFailure());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkExceptionThrown_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkExceptionThrown());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkShortCircuited_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkShortCircuited());

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunctionMock.Object));

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await Assert.ThrowsAsync<HystrixCommandException>(() => hystrixCommand.ExecuteAsync(primaryFunction));

                commandMetricsMock.VerifyAll();
            }
        }
        
        public class ExecuteAsyncWithFallback
        {
            private readonly Mock<IHystrixTimeoutWrapper> timeoutWrapperMock;
            private readonly Mock<IHystrixCircuitBreaker> circuitBreakerMock;
            private readonly Mock<IHystrixCommandMetrics> commandMetricsMock;
            private readonly Mock<IHystrixThreadPoolMetrics> threadPoolMetricsMock;
            private readonly Mock<IHystrixConfigurationService> configurationServiceMock;
            private readonly HystrixCommand hystrixCommand;
            
            public ExecuteAsyncWithFallback()
            {
                timeoutWrapperMock = new Mock<IHystrixTimeoutWrapper>();
                circuitBreakerMock = new Mock<IHystrixCircuitBreaker>();
                commandMetricsMock = new Mock<IHystrixCommandMetrics>();
                threadPoolMetricsMock = new Mock<IHystrixThreadPoolMetrics>();
                configurationServiceMock = new Mock<IHystrixConfigurationService>();

                hystrixCommand = new HystrixCommand(new HystrixCommandIdentifier("group", "command"), timeoutWrapperMock.Object, circuitBreakerMock.Object, commandMetricsMock.Object, threadPoolMetricsMock.Object, configurationServiceMock.Object);
            }

            [Fact]
            public async Task Runs_Primary_Function_Directly_When_HystrixCommandEnabled_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(false);

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                Assert.Equal("a value", value);
            }

            [Fact]
            public async Task Runs_The_Primary_Function_In_The_TimeoutWrapper_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Returns((Func<Task<string>> primary, CancellationTokenSource cancellationTokenSource) => primary());

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                Assert.Equal("a value", value);
                timeoutWrapperMock.VerifyAll();
            }

            [Fact]
            public async Task Runs_Fallback_Function_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public async Task Runs_Fallback_Function_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public async Task Runs_Fallback_Function_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());

                // Act
                string value = await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                Assert.Equal("an alternative value", value);
            }

            [Fact]
            public async Task Calls_IncrementConcurrentExecutionCount_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.IncrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadExecution_When_Allow_Request_Is_True()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadExecution());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Opens_CircuitBreaker_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                circuitBreakerMock.Setup(x => x.CloseCircuit());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                circuitBreakerMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddCommandExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddCommandExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkSuccess_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.MarkSuccess());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkTimeout_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.MarkTimeout());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkFailure_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkFailure());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkExceptionThrown_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.MarkExceptionThrown());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_DecrementConcurrentExecutionCount_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.DecrementConcurrentExecutionCount());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkThreadCompletion_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                threadPoolMetricsMock.Setup(x => x.MarkThreadCompletion());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                threadPoolMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkShortCircuited_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkShortCircuited());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_True_And_Primary_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_TimeoutWrapper_Throws_HystrixTimeoutException()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunction, null)).Throws<HystrixTimeoutException>();
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Primary_Function_Throws_Any_Exception()
            {
                var primaryFunctionMock = new Mock<Func<Task<string>>>();
                primaryFunctionMock.Setup(x => x()).Throws<Exception>();
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(true);
                timeoutWrapperMock.Setup(x => x.ExecuteAsync(primaryFunctionMock.Object, null)).Returns((Func<Task<string>> primary) => primary());
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunctionMock.Object, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_AddUserThreadExecutionTime_When_Allow_Request_Is_False()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.AddUserThreadExecutionTime(It.Is<double>(y => y >= 0)));

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Calls_MarkFallbackSuccess_When_Fallback_Function_Succeeds()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                commandMetricsMock.Setup(x => x.MarkFallbackSuccess());

                // Act
                await hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction);

                commandMetricsMock.VerifyAll();
            }

            [Fact]
            public async Task Does_Not_CallsMarkFallbackSuccess_When_Fallback_Function_Throws_Any_Exception()
            {
                var primaryFunction = new Func<Task<string>>(() => Task.FromResult("a value"));
                var fallbackFunction = new Func<Task<string>>(() => Task.FromResult("an alternative value"));

                var fallbackFunctionMock = new Mock<Func<string>>();
                fallbackFunctionMock.Setup(x => x()).Throws<Exception>();

                configurationServiceMock.Setup(x => x.GetHystrixCommandEnabled()).Returns(true);
                circuitBreakerMock.Setup(x => x.AllowRequest()).Returns(false);
                // http://stackoverflow.com/questions/537308/how-to-verify-that-method-was-not-called-in-moq
                commandMetricsMock.Setup(x => x.MarkFallbackSuccess()).Throws(new Exception("Shouldn't be called."));

                // Act
                await Assert.ThrowsAsync<Exception>(() => hystrixCommand.ExecuteAsync(primaryFunction, fallbackFunction));
            }
        }
    }
}
