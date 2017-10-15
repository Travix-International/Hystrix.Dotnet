using Hystrix.Dotnet.Logging;
using Hystrix.Dotnet.Metrics;
using Hystrix.Dotnet.WebConfiguration;
using Owin;
using System.Configuration;

namespace Hystrix.Dotnet.Owin
{
    public static class HystrixStreamExtensions
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixStreamExtensions));


        public static IAppBuilder UseHystrixStream(this IAppBuilder app)
        {
            var helper = new AspNetHystrixCommandFactoryHelper();

            return app.UseHystrixStream(helper.CreateFactory());
        }

        public static IAppBuilder UseHystrixStream(this IAppBuilder app, IHystrixCommandFactory hystrixCommandFactory)
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            int pollingInterval = configSection?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            log.InfoFormat("Creating HystrixStreamHandler with interval {0}", pollingInterval);

            var endpoint = new HystrixMetricsStreamEndpoint(
                hystrixCommandFactory,
                pollingInterval);

            return app.UseHystrixStream(endpoint);
        }

        public static IAppBuilder UseHystrixStream(this IAppBuilder app, IHystrixMetricsStreamEndpoint endpoint)
        {
            app.Use<HystrixStreamMiddleware>(endpoint);

            return app;
        }
    }
}
