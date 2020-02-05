# Using Hystrix.Dotnet in ASP.NET Core

In order to use Hystrix.Dotnet in ASP.NET Core we need to do the following steps.

Add a dependency to the package `Hystrix.Dotnet.AspNetCore` in our project.json.

```
  "dependencies": {
    "Hystrix.Dotnet.AspNetCore": "1.0.0-beta3",
    ...
  }
```

(Optional) Add our custom configuration to the `appsettings.json` file.

```json
{
    "Hystrix": {
        "ConfigurationServiceImplementation": "HystrixLocalConfigurationService",
        "MetricsStreamPollIntervalInMilliseconds": 2000,
        "LocalOptions": {
            "CommandGroups": {
                "TestGroup": {
                    "TestCommand": {
                        "CommandTimeoutInMilliseconds": 1250,
                        "CircuitBreakerErrorThresholdPercentage": 60
                    }
                }
            },
            "DefaultOptions": {
                "CommandTimeoutInMilliseconds": 1500
            }
        }
    }
}
```

Set up our `Startup` type to use Hystrix. We need to

 - Call `AddHystrix` to add the necessary services to DI.
 - (Optional) call `Configure` to bind the options coming from the `appsettings.json`.
 - (Optional) call `UseHystrixMetricsEndpoint()` to set up the metrics streaming endpoint.

```csharp
public class Startup
{
    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json");
        Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddHystrix();
        services.Configure<HystrixOptions>(options => Configuration.GetSection("Hystrix").Bind(options));
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        ...
        app.UseHystrixMetricsEndpoint("hystrix.stream");
    }
}
```

Then we can simply inject the `IHystrixCommandFactory` into our controller (or other component), and get the reference to a particular command.

```csharp
    public class HelloController : Controller
    {
        private readonly IHystrixCommand testHystrixCommand;
        
        public HelloController(IHystrixCommandFactory hystrixCommandFactory)
        {
            testHystrixCommand = hystrixCommandFactory.GetHystrixCommand("TestGroup", "TestCommand");
        }
        
        ...
    }
```