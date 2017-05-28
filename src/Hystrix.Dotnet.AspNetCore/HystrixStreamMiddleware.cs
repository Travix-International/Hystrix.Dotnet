using System.Threading.Tasks;
using Hystrix.Dotnet.Metrics;
using Hystrix.Dotnet.Logging;
using Microsoft.AspNetCore.Http;

namespace Hystrix.Dotnet.AspNetCore
{
    public class HystrixStreamMiddleware
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixStreamMiddleware));

        // ReSharper disable once UnusedParameter.Local, needed by the ASP.NET Core framework.
        public HystrixStreamMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context, IHystrixMetricsStreamEndpoint streamEndpoint)
        {
            log.Info("Starting HystrixStreamHandler request");

            var response = context.Response;

            // Do not cache
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Expires", "-1");
            response.Headers.Add("Pragma", "no-cache");

            response.ContentType = "text/event-stream";

            await streamEndpoint.PushContentToOutputStream(response.Body, () => { }, context.RequestAborted).ConfigureAwait(false);

            log.Info("Ending HystrixStreamMiddleware request");
        }
    }
}