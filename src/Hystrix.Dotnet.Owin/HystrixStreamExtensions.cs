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
            return app.UseHystrixStream("/hystrix.stream");
        }

        public static IAppBuilder UseHystrixStream(
            this IAppBuilder app,
            string route)
        {
            var helper = new AspNetHystrixCommandFactoryHelper();

            return app.UseHystrixStream(helper.CreateFactory(), route);
        }

        public static IAppBuilder UseHystrixStream(this IAppBuilder app,
            IHystrixCommandFactory hystrixCommandFactory,
            string route)
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            int pollingInterval = configSection?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            log.InfoFormat("Creating HystrixStreamHandler with interval {0}", pollingInterval);

            var endpoint = new HystrixMetricsStreamEndpoint(
                hystrixCommandFactory,
                pollingInterval);

            return app.UseHystrixStream(endpoint, route);
        }

        public static IAppBuilder UseHystrixStream(
            this IAppBuilder app,
            IHystrixMetricsStreamEndpoint endpoint,
            string route)
        {
            app.Use<HystrixStreamMiddleware>(endpoint, route);

            return app;
        }
    }
}
