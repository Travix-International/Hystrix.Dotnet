using Hystrix.Dotnet.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using log4net.Config;
using System.IO;
using System.Reflection;
using Serilog;

namespace Hystrix.Dotnet.Samples.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddHystrix();
            
            services.Configure<HystrixOptions>(options => Configuration.GetSection("Hystrix").Bind(options));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            // Set up a Serilog log sink.
            var log = new LoggerConfiguration()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                .CreateLogger();
            Log.Logger = log;

            app.UseMvc();
            app.UseHystrixMetricsEndpoint("hystrix.stream");
        }
    }
}