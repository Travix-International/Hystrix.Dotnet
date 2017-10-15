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

        public HystrixStreamMiddleware(OwinMiddleware next,
            IHystrixMetricsStreamEndpoint endpoint) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/hystrix.stream")))
                await base.Next.Invoke(context);
            else
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
