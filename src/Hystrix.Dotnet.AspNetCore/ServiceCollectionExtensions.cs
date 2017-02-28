using Hystrix.Dotnet.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hystrix.Dotnet.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHystrix(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(s =>
            {
                var options = s.GetService<IOptions<HystrixOptions>>();

                return options?.Value ?? HystrixOptions.CreateDefault();
            });
            serviceCollection.AddSingleton<IHystrixCommandFactory, HystrixCommandFactory>();
            serviceCollection.AddSingleton<IHystrixMetricsStreamEndpoint, HystrixMetricsStreamEndpoint>();
        }
    }
}