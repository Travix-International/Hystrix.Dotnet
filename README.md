# Hystrix.Dotnet

A combination of circuit breaker and timeout. The .NET version of the open source [Hystrix library](https://github.com/Netflix/Hystrix) built by Netflix.

[![Build Status .NET](https://ci.appveyor.com/api/projects/status/github/Travix-International/Hystrix.Dotnet?svg=true)](https://ci.appveyor.com/project/Travix-International/Hystrix.Dotnet/)
[![Version](https://img.shields.io/nuget/v/Hystrix.Dotnet.svg)](https://www.nuget.org/packages/Hystrix.Dotnet)
[![Coverage Status](https://coveralls.io/repos/github/Travix-International/Hystrix.Dotnet/badge.svg?branch=upgrade-to-net-core)](https://coveralls.io/github/Travix-International/Hystrix.Dotnet?branch=upgrade-to-net-core)
[![License](https://img.shields.io/github/license/Travix-International/Hystrix.Dotnet.svg)](https://github.com/Travix-International/Hystrix.Dotnet/blob/master/LICENSE)

## Why?

In order to isolate failure in one dependency from taking down another component. Whenever the circuit breaker opens it returns an exception or runs the fallback without burdening the failing system. It sends through a single request on a regular interval to see if the dependent system is back in business.

## Usage

The circuit breakers are identifyable by group and command key. To make sure you get the same Hystrix command object for each group and command key combination you should use the factory the retrieve the command.

### Creating the factory manually

```csharp
var options = HystrixOptions.CreateDefault();
var hystrixCommandFactory = new HystrixCommandFactory(options);
var hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
```

### Creating the factory in ASP.NET

In ASP.NET we can use the `AspNetHystrixCommandFactoryHelper` helper class to create our factory, which will automatically pick up the configuration from the web.config.

```csharp
var helper = new AspNetHystrixCommandFactoryHelper();
var factory = helper.CreateFactory();
var hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
```

### Creating the factory in ASP.NET Core

With ASP.NET Core we can leverage the built-in dependency injection, so we won't need a helper class, we can simply inject the factory into our controllers.

In our `Startup` class we have to call the `AddHystrix()` method on our service collection.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddHystrix();
}
```

And then inject the `IHystrixCommandFactory` into our controller.

```csharp
public class ExampleController : Controller
{
    private readonly IHystrixCommandFactory hystrixCommandFactory;
    
    public ExampleController(IHystrixCommandFactory hystrixCommandFactory)
    {
        this.hystrixCommandFactory = hystrixCommandFactory;
    }

    public IActionResult Get()
    {
        ...
        var hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
        ...
    }
```

The command is a combination of circuit breaker and timeout pattern. To wrap your function with it use either the sync version:

```csharp
T result = hystrixCommand.Execute<T>(() => mySyncFunctionWithReturnTypeT());
```

Or use the async version

```csharp
T result = await hystrixCommand.ExecuteAsync<T>(() => myAsyncFunctionWithReturnTypeT());
```

Furthermore, a fallback can be defined as a second lambda expression right after the encapsulated command. This way e.g. in case of a failing network connection the fallback can return some cached or default value.

```csharp
T result = hystrixCommand.Execute<T>(
    () => mySyncFunctionWithReturnTypeT(), 
    () => mySyncFallbackFunctionWithReturnTypeT()
);

T result = await hystrixCommand.ExecuteAsync<T>(
    () => myAsyncFunctionWithReturnTypeT(), 
    () => myAsyncFallbackFunctionWithReturnTypeT()
);
```

## Configuration

All the configuration parameters are controller via an `IOptions<HystrixOptions>` object, which is passed in to the `HystrixCommandFactory`.
There are two main modes of configuring our individual commands, based on the value of the `ConfigurationServiceImplementation` property.
 - If the value is **HystrixLocalConfigConfigurationService**, then the configuration is read from the `LocalOptions` property, which is populated from the configuration deployed with the application. (in ASP.NET it is coming from the web.config, while  in ASP.NET Core, it's typically in an appsettings.json file.)
 - If the value is **HystrixJsonConfigConfigurationService**, then the configuration is retrieved from an external service, so that it can by dynamically changed in near real time.

 ### Using local (deployed) configuration

 #### ASP.NET

 With ASP.NET, we have to add the following configuration to our web.config.

```xml
<configuration>
  <configSections>
    <sectionGroup name="hystrix.dotnet">
      <section name="hystrix" type="Hystrix.Dotnet.AspNet.HystrixConfigSection, Hystrix.Dotnet.AspNet" />
    </sectionGroup>
  </configSections>

  <hystrix.dotnet>
    <hystrix serviceImplementation="HystrixLocalConfigurationService" metricsStreamPollIntervalInMilliseconds="2000">
      <localOptions>
        <commandGroups>
          <add key="GroupKey">
            <commands>
              <add key="CommandKey"
                hystrixCommandEnabled="true"
                commandTimeoutInMilliseconds="1250"
                circuitBreakerForcedOpen="false"
                circuitBreakerForcedClosed="false"
                circuitBreakerErrorThresholdPercentage="50"
                circuitBreakerSleepWindowInMilliseconds="5000"
                circuitBreakerRequestVolumeThreshold="20"
                metricsHealthSnapshotIntervalInMilliseconds="500"
                metricsRollingStatisticalWindowInMilliseconds="10000"
                metricsRollingStatisticalWindowBuckets="10"
                metricsRollingPercentileEnabled="true"
                metricsRollingPercentileWindowInMilliseconds="60000"
                metricsRollingPercentileWindowBuckets="6"
                metricsRollingPercentileBucketSize="100" />
            </commands>
          </add>
        </commandGroups>
      </localOptions>
    </hystrix>
  </hystrix.dotnet>
```

This way we can add multiple groups and commands, and fine-tune each independently.
The above values are also the defaults if we omit any of the attributes. (Or if we don't add any configuration to the web.config.)

#### ASP.NET Core

With ASP.NET Core we can use the Options configuration model to do the same setup.
First add an **appsettings.json** to our project, with the following content.

```json
{
    "Hystrix": {
        "ConfigurationServiceImplementation": "HystrixLocalConfigurationService",
        "MetricsStreamPollIntervalInMilliseconds": 2000,
        "LocalOptions": {
            "CommandGroups": {
                "GroupKey": {
                    "CommandKey": {
                        "HystrixCommandEnabled": true,
                        "CommandTimeoutInMilliseconds": 1250,
                        "CircuitBreakerForcedOpen": false,
                        "CircuitBreakerForcedClosed": false,
                        "CircuitBreakerErrorThresholdPercentage": 60,
                        "CircuitBreakerSleepWindowInMilliseconds": 5000,
                        "CircuitBreakerRequestVolumeThreshold": 20,
                        "MetricsHealthSnapshotIntervalInMilliseconds": 500,
                        "MetricsRollingStatisticalWindowInMilliseconds": 10000,
                        "MetricsRollingStatisticalWindowBuckets": 10,
                        "MetricsRollingPercentileEnabled": true,
                        "MetricsRollingPercentileWindowInMilliseconds": 60000,
                        "MetricsRollingPercentileWindowBuckets": 6,
                        "MetricsRollingPercentileBucketSize": 100
                    }
                }
            }
        }
    }
}
```

Then set up the options object in our DI configuration, in the `ConfigureServices` method of the `Startup` class.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.Configure<HystrixOptions>(options => Configuration.GetSection("Hystrix").Bind(options));
}
```

### Using external Json Configuration

In order to be able to control the settings near realtime you're better off using the **HystrixJsonConfigConfigurationService**. It can fetch a json object containing the configuration from a remote url.

We have to publish the configuration to a URL (http://hystrix-config.mydomain.com/Hystrix-GroupKey-CommandKey.json) in the following Json format.

```json
{
    "HystrixCommandEnabled": true,
    "CommandTimeoutInMilliseconds":1000,
    "CircuitBreakerForcedOpen":false,
    "CircuitBreakerForcedClosed":false,
    "CircuitBreakerErrorThresholdPercentage":50,
    "CircuitBreakerSleepWindowInMilliseconds":5000,
    "CircuitBreakerRequestVolumeThreshold":20,
    "MetricsHealthSnapshotIntervalInMilliseconds":500,
    "MetricsRollingStatisticalWindowInMilliseconds":10000,
    "MetricsRollingStatisticalWindowBuckets":10,
    "MetricsRollingPercentileEnabled":true,
    "MetricsRollingPercentileWindowInMilliseconds":60000,
    "MetricsRollingPercentileWindowBuckets":6,
    "MetricsRollingPercentileBucketSize":100
}
```

And configure Hystrix to use it instead of the local configuration.
In ASP.NET web.config.

```xml
<hystrix.dotnet>
<hystrix serviceImplementation="HystrixJsonConfigConfigurationService" metricsStreamPollIntervalInMilliseconds="2000">
  <jsonConfigurationSourceOptions
    pollingIntervalInMilliseconds="5000" 
    locationPattern="Hystrix-{0}-{1}.json"
    baseLocation="http://hystrix-config.mydomain.com/" />
</hystrix>
</hystrix.dotnet>
```

Or in the appsettings.json in ASP.NET Core.

```json
{
    "Hystrix": {
        "ConfigurationServiceImplementation": "HystrixJsonConfigConfigurationService",
        "JsonConfigurationSourceOptions": {
            "PollingIntervalInMilliseconds": 5000,
            "LocationPattern": "Hystrix-{0}-{1}.json",
            "BaseLocation":"http://hystrix-config.mydomain.com/"
        }
    }
}
```

The first time a Hystrix command is created by the factory, it waits for the remote config to be fetched. After that it updates in a non-blocking way using a background thread at an interval defined by the earlier appsetting. It will also fail silently continuing to use the last known configuration.

For more info on the configuration see https://github.com/Netflix/Hystrix/wiki/configuration and https://github.com/Netflix/Hystrix/wiki/Operations on how to tune it for first use.

## Visualizing metrics

In order to expose the metrics of all of your Hystrix commands, we need to publish them in our web application. In ASP.NET we have to use a handler, while in ASP.NET Core we should add a middleware to our pipeline.

### ASP.NET

In ASP.NET we can publish the metrics stream by adding the **HystrixStreamHandler** to our application.

```xml
<system.webServer>
    <handlers>
        ...
        <add name="HystrixStreamHandler" verb="*" path="hystrix.stream" type="Hystrix.Dotnet.AspNet.HystrixStreamHandler" preCondition="integratedMode,runtimeVersionv4.0" />
        ...
    </handlers>
</add>system.webServer>
```

For both MVC and WebApi application the path `/hystrix.stream` will be picked up by either MVC or WebApi instead of the handler. To make sure a request makes its way to the handler add the following Ignore to your **global.asax.cs**:

```csharp
protected void Application_Start()
{
    // ignore route for hystrix.stream httphandler
    RouteTable.Routes.Ignore("hystrix.stream/{*pathInfo}");

    ...
}
```

### ASP.NET Core

In ASP.NET Core we have to add a middleware to our pipeline to publish the metrics. We can do this in the `Configure` method of our `Startup` class.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    ...
    app.UseHystrixMetricsEndpoint("hystrix.stream");
}
```

### The metrics stream

The hystrix.stream is a **text/event-stream** that pushes the information from the server to the requester of the url. It does this at an interval defined by the **MetricsStreamPollIntervalInMilliseconds** configuration parameter, which we can specify in our web.config or appsettings.json file.

In order to see your Hystrix command in action spin up the following docker container locally:

```
docker run -d -p 8080:8080 travix/hystrix-dashboard
```

And then in the dashboard - at http://192.168.99.100:8080/ in case you use Kitematic - paste the following url where <mylocalip> points to the web application running on your local machine.

```
http://<mylocalip>/hystrix.stream
```

When requesting urls for your local application that hit your Hystrix command it should show up in the dashboard. For more info on how to read the dashboard, see https://github.com/Netflix/Hystrix/wiki/Dashboard

### Logging

Hystrix.Dotnet logs some diagnostic information using LibLog. You can hook these log messages into logging libraries like log4net, NLog or Serilog, or you can provide your own logging provider implementation.

You can find an example of setting the logging up for Serilog in the [ASP.NET Core sample](/samples/Hystrix.Dotnet.Samples.AspNetCore), and you can find examples for other logging libraries in the [LibLog project](https://github.com/damianh/LibLog).

## Sample projects

In the [samples](/samples) directory you can find an example project illustrating the configuration of Hystrix for [ASP.NET](/samples/Hystrix.Dotnet.Samples.AspNet) and [ASP.NET Core](/samples/Hystrix.Dotnet.Samples.AspNetCore).

## Known issues

Unlike the original Hystrix implementation the current .Net implementation doesn't use a way to limit the maximum number of concurrent requests per command. Using the ExecuteAsync method will make efficient use of the threadpool, so it's not entirely clear whether it will give us any benefits.

Neither are retries implemented at this moment.
