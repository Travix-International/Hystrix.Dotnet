using Microsoft.AspNetCore.Builder;

namespace Hystrix.Dotnet.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseHystrixMetricsEndpoint(this IApplicationBuilder builder, string route)
        {
            builder.Map(route.StartsWith("/") ? route : $"/{route}", a => a.UseMiddleware<HystrixStreamMiddleware>());
        }
    }
}