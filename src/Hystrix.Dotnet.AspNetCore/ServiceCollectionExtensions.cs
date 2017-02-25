using Hystrix.Dotnet;
using Microsoft.Extensions.DependencyInjection;

namespace Hystrix.Dotnet.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHystrix(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHystrixCommandFactory, HystrixCommandFactory>();
            serviceCollection.AddSingleton<IHystrixMetricsStreamEndpoint, HystrixMetricsStreamEndpoint>();
        }
    }
}