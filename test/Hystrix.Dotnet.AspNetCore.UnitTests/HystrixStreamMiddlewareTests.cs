using System.Threading.Tasks;
using Hystrix.Dotnet.Metrics;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.AspNetCore.UnitTests
{
    public class HystrixStreamMiddlewareTests
    {
        [Fact]
        public async Task Invoke_Called_ResponseHeadersCorrectlySet()
        {
            var sut = new HystrixStreamMiddleware(context => Task.CompletedTask);
            var metricsStreamEndpointMock = new Mock<IHystrixMetricsStreamEndpoint>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var headerDictionaryMock = new Mock<IHeaderDictionary>();

            responseMock.SetupGet(r => r.Headers).Returns(headerDictionaryMock.Object);
            httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

            await sut.Invoke(httpContextMock.Object, metricsStreamEndpointMock.Object);
        }
    }
}