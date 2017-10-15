using Hystrix.Dotnet.Logging;
using Hystrix.Dotnet.Metrics;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace Hystrix.Dotnet.Owin
{
    class HystrixStreamMiddleware : OwinMiddleware
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixStreamMiddleware));
        private readonly IHystrixMetricsStreamEndpoint endpoint;
        private readonly string route;

        public HystrixStreamMiddleware(OwinMiddleware next,
            IHystrixMetricsStreamEndpoint endpoint,
            string route) : base(next)
        {
            this.endpoint = endpoint;
            this.route = route.StartsWith("/") ? route : $"/{route}";
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!context.Request.Path.Value.StartsWith(this.route))
            {
                await Next.Invoke(context);
                return;
            }

            await ProcessHystrix(context);
        }

        async Task ProcessHystrix(IOwinContext context)
        {
            log.Info("Starting HystrixStreamHandler request");
            var response = context.Response;

            // do not cache
            response.Headers.Add("Cache-Control", new[] { "no-cache" });
            response.Headers.Add("Expires", new[] { "-1" });
            response.Headers.Add("Pragma", new[] { "no-cache" });

            response.ContentType = "text/event-stream";

            await endpoint.PushContentToOutputStream(response.Body, () => { }, context.Request.CallCancelled).ConfigureAwait(false);
        }
    }
}
