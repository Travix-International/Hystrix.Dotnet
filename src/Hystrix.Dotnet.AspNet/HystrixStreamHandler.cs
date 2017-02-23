using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixStreamHandler : HttpTaskAsyncHandler
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixStreamHandler));

        private readonly IHystrixMetricsStreamEndpoint endpoint;

        private const string pollingIntervalInMilliseconds = "HystrixStreamHandler-PollingIntervalInMilliseconds";

        public HystrixStreamHandler()
        {
            int pollingInterval;
            if (!int.TryParse(ConfigurationManager.AppSettings[pollingIntervalInMilliseconds], out pollingInterval))
            {
                pollingInterval = 500;
            }

            //Log.InfoFormat("Creating HystrixStreamHandler with interval {0}", pollingInterval);

            // TODO: Instantiate the options based on the ConfigurationManager
            endpoint = new HystrixMetricsStreamEndpoint(new HystrixCommandFactory(Options.Create(new HystrixOptions())), pollingInterval);
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            //Log.Info("Starting HystrixStreamHandler request");

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

            await endpoint.PushContentToOutputStream(new HttpResponseWrapper(response)).ConfigureAwait(false);

            //Log.Info("Ending HystrixStreamHandler request");
        }

        public override bool IsReusable => false;

        public override void ProcessRequest(HttpContext context)
        {
            throw new Exception("The ProcessRequest method has no implementation.");
        }
    }
}