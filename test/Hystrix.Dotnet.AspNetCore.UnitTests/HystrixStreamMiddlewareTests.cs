using System;
using System.IO;
using System.Threading;
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

            headerDictionaryMock.Verify(h => h.Add("Cache-Control", "no-cache"));
            headerDictionaryMock.Verify(h => h.Add("Expires", "-1"));
            headerDictionaryMock.Verify(h => h.Add("Pragma", "no-cache"));

            responseMock.VerifySet(r => r.ContentType = "text/event-stream");
        }

        [Fact]
        public async Task Invoke_Called_MetricsEndpointCalled()
        {
            var sut = new HystrixStreamMiddleware(context => Task.CompletedTask);
            var metricsStreamEndpointMock = new Mock<IHystrixMetricsStreamEndpoint>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();

            var cancellationToken = new CancellationToken();
            httpContextMock.SetupGet(r => r.RequestAborted).Returns(cancellationToken);

            httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

            var stream = new MemoryStream();
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            responseMock.SetupGet(r => r.Body).Returns(stream);
            responseMock.SetupGet(r => r.Headers).Returns(headerDictionaryMock.Object);

            await sut.Invoke(httpContextMock.Object, metricsStreamEndpointMock.Object);

            metricsStreamEndpointMock.Verify(e => e.PushContentToOutputStream(stream, It.IsAny<Action>(), cancellationToken));
        }
    }
}