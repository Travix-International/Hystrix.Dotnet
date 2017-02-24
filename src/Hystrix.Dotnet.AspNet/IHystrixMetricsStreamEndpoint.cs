using System.Threading.Tasks;
using System.Web;

namespace Hystrix.Dotnet.AspNet
{
    internal interface IHystrixMetricsStreamEndpoint
    {
        Task PushContentToOutputStream(HttpResponseBase response);
    }
}