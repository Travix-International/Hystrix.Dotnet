using Hystrix.Dotnet;
using Hystrix.Dotnet.Logging;
using Hystrix.Dotnet.Metrics;
using Hystrix.Dotnet.Owin;
using Hystrix.Dotnet.WebConfiguration;
using Owin;
using System.Configuration;

namespace Owin
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

            return app.UseHystrixStream(route, helper.CreateFactory());
        }

        public static IAppBuilder UseHystrixStream(this IAppBuilder app,
            string route,
            IHystrixCommandFactory hystrixCommandFactory)
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            int pollingInterval = configSection?.MetricsStreamPollIntervalInMilliseconds ?? 500;

            log.InfoFormat("Creating HystrixStreamHandler with interval {0}", pollingInterval);

            var endpoint = new HystrixMetricsStreamEndpoint(
                hystrixCommandFactory,
                pollingInterval);

            return app.UseHystrixStream(route, endpoint);
        }

        public static IAppBuilder UseHystrixStream(
            this IAppBuilder app,
            string route,
            IHystrixMetricsStreamEndpoint endpoint)
        {
            app.Use<HystrixStreamMiddleware>(endpoint, route);

            return app;
        }
    }
}
