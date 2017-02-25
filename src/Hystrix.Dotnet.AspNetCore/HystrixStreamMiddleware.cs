using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Hystrix.Dotnet.AspNetCore
{
    public class HystrixStreamMiddleware
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HystrixStreamMiddleware));

        // ReSharper disable once UnusedParameter.Local, needed by the ASP.NET Core framework.
        public HystrixStreamMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context, IOptions<HystrixOptions> options)
        {
            int pollingInterval = options.Value?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            var endpoint = new HystrixMetricsStreamEndpoint(
                new HystrixCommandFactory(options), 
                pollingInterval);

            log.Info("Starting HystrixStreamHandler request");

            var response = context.Response;

            // Do not cache
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Expires", "-1");
            response.Headers.Add("Pragma", "no-cache");

            response.ContentType = "text/event-stream";

            await endpoint.PushContentToOutputStream(response).ConfigureAwait(false);

            log.Info("Ending HystrixStreamHandler request");
        }
    }
}