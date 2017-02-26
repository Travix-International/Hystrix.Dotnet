using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet.Metrics
{
    public interface IHystrixMetricsStreamEndpoint
    {
        Task PushContentToOutputStream(Stream outputStream, Action flushResponse, CancellationToken cancellationToken);
    }
}