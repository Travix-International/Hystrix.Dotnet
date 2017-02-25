using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Hystrix.Dotnet.AspNetCore
{
    public interface IHystrixMetricsStreamEndpoint
    {
        Task PushContentToOutputStream(HttpResponse response);
    }
}