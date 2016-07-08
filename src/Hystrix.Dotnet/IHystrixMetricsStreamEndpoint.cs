#if !COREFX

using System.Threading.Tasks;
using System.Web;

namespace Hystrix.Dotnet
{
    internal interface IHystrixMetricsStreamEndpoint
    {
        Task PushContentToOutputStream(HttpResponseBase response);
        Task WriteAllCommandsJsonToOutputStream(HttpResponseBase response);
        Task<string> GetCommandJson(IHystrixCommand command);
    }
}

#endif