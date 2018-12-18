using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Hystrix.Dotnet.Metrics;
using Hystrix.Dotnet.Logging;
using Hystrix.Dotnet.WebConfiguration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixStreamHandler : HttpTaskAsyncHandler
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixStreamHandler));

        private readonly IHystrixMetricsStreamEndpoint endpoint;

        public HystrixStreamHandler()
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            int pollingInterval = configSection?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            log.InfoFormat("Creating HystrixStreamHandler with interval {0}", pollingInterval);

            var helper = new AspNetHystrixCommandFactoryHelper();

            endpoint = new HystrixMetricsStreamEndpoint(
                helper.CreateFactory(),
                pollingInterval);
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            log.Info("Starting HystrixStreamHandler request");

            var response = context.Response;

            response.Clear();

            // do not cache
            response.AppendHeader("Cache-Control", "no-cache");
            response.AppendHeader("Expires", "-1");
            response.AppendHeader("Pragma", "no-cache");

            response.ContentType = "text/event-stream";

            // make sure it's non buffered, but outputstream is directly written; this automatically sets Transfer-Encoding: chunked 
            response.Buffer = false;
            response.BufferOutput = false;

            // flush the headers
            response.Flush();

            await endpoint.PushContentToOutputStream(response.OutputStream, () => response.Flush(), response.ClientDisconnectedToken).ConfigureAwait(false);

            log.Info("Ending HystrixStreamHandler request");
        }

        public override bool IsReusable => false;

        public override void ProcessRequest(HttpContext context)
        {
            throw new Exception("The ProcessRequest method has no implementation.");
        }
    }
}