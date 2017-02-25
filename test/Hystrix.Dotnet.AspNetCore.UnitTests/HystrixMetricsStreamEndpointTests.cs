using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.AspNetCore.UnitTests
{
    public class HystrixMetricsStreamEndpointTests
    {
        public class Constructor
        {
            public Constructor()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public void Throws_ArgumentNullException_When_CommandFactory_Is_Null()
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixMetricsStreamEndpoint(null, Options.Create(new HystrixOptions())));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_PollingInterval_Is_Less_Than_100()
            {
                var commandFactoryMock = new Mock<IHystrixCommandFactory>();

                var invalidOptions = new HystrixOptions
                {
                    MetricsStreamPollIntervalInMilliseconds = 50
                };

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixMetricsStreamEndpoint(commandFactoryMock.Object, Options.Create(invalidOptions)));
            }
        }

        public class GetCommandJson
        {
            public GetCommandJson()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public void Returns_Json_String_In_Hystrix_Format_For_HystrixCommand()
            {
                var options = Options.Create(HystrixOptions.CreateDefault());
                HystrixCommandFactory commandFactory = new HystrixCommandFactory(options);
                var endpoint = new HystrixMetricsStreamEndpoint(commandFactory, options);
                var hystrixCommand = commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                // Act
                var commandJson = endpoint.GetCommandJson(hystrixCommand);

                Assert.NotNull(commandJson);
            }
        }

        public class WriteAllCommandsJsonToOutputStream
        {
            public WriteAllCommandsJsonToOutputStream()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public async Task Writes_Command_Json_For_All_HystrixCommands_To_OutputStream()
            {
                var options = Options.Create(HystrixOptions.CreateDefault());
                HystrixCommandFactory commandFactory = new HystrixCommandFactory(options);
                var endpoint = new HystrixMetricsStreamEndpoint(commandFactory, options);
                commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));
                commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandY"));
                var httpResponseMock = new Mock<HttpResponse>();
                httpResponseMock.Setup(x => x.Body).Returns(new MemoryStream());

                // Act
                await endpoint.WriteAllCommandsJsonToOutputStream(httpResponseMock.Object);
            }
        }
    }
}