using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
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
                int pollingInterval = 1000;

                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixMetricsStreamEndpoint(null, pollingInterval));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_PollingInterval_Is_Less_Than_100()
            {
                var commandFactoryMock = new Mock<IHystrixCommandFactory>();

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixMetricsStreamEndpoint(commandFactoryMock.Object, 99));
            }
        }

        public class GetCommandJson
        {
            public GetCommandJson()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public async Task Returns_Json_String_In_Hystrix_Format_For_HystrixCommand()
            {
                HystrixCommandFactory commandFactory = new HystrixCommandFactory();
                int pollingInterval = 1000;
                var endpoint = new HystrixMetricsStreamEndpoint(commandFactory, pollingInterval);
                var hystrixCommand = commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                // act
                var commandJson = await endpoint.GetCommandJson(hystrixCommand);

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
                HystrixCommandFactory commandFactory = new HystrixCommandFactory();
                int pollingInterval = 1000;
                var endpoint = new HystrixMetricsStreamEndpoint(commandFactory, pollingInterval);
                commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));
                commandFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandY"));
                var httpResponseMock = new Mock<HttpResponseBase>();
                httpResponseMock.Setup(x => x.OutputStream).Returns(new MemoryStream());

                // act
                await endpoint.WriteAllCommandsJsonToOutputStream(httpResponseMock.Object);
            }
        }
    }
}
