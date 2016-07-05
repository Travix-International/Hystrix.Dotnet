# Hystrix.Dotnet

A combination of circuit breaker and timeout. The .net version of the open source [Hystrix library](https://github.com/Netflix/Hystrix library) built by Netflix.

[![Build Status .NET](https://ci.appveyor.com/api/projects/status/github/Travix-International/Hystrix.Dotnet?svg=true)](https://ci.appveyor.com/project/Travix-International/Hystrix.Dotnet/)
[![Version](https://img.shields.io/nuget/v/Hystrix.Dotnet.svg)](https://www.nuget.org/packages/Hystrix.Dotnet)
[![License](https://img.shields.io/github/license/Travix-International/Hystrix.Dotnet.svg)](https://github.com/Travix-International/Hystrix.Dotnet/blob/master/LICENSE)

Why?
--------------------------------

In order to isolate failure in one dependency from taking down another component. Whenever the circuit breaker opens it returns an exception or runs the fallback without burdening the failing system. It sends through a single request on a regular interval to see if the dependent system is back in business.

Usage
--------------------------------

The circuit breakers are identifyable by group and command key. To make sure you get the same Hystrix command object for each group and command key combination you should use the factory the retrieve the command:

```csharp
IHystrixCommandFactory hystrixCommandFactory = new HystrixCommandFactory();
IHystrixCommand hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
```

The command is a combination of circuit breaker and timeout pattern. To wrap your function with it use either the sync version:

```csharp
T result = hystrixCommand.Execute<T>(() => mySyncFunctionWithReturnTypeT());
```

Or use the async version

```csharp
T result = await hystrixCommand.ExecuteAsync<T>(() => myAsyncFunctionWithReturnTypeT());
```

Configuration
--------------------------------

In order to control the configuration of each individual Hystrix command you can either use **HystrixWebConfigConfigurationService** to retrieve settings from web.config appsettings:

```
<add key="HystrixCommandFactory-ConfigurationServiceImplementation" value="HystrixWebConfigConfigurationService" />
```

And then for each group define the following appsettings. The below values are also the defaults used when the appsettings are missing.

```
<add key="GroupKey-CommandKey-HystrixCommandEnabled" value="true" />
<add key="GroupKey-CommandKey-CommandTimeoutInMilliseconds" value="1000" />
<add key="GroupKey-CommandKey-CircuitBreakerForcedOpen" value="false" />
<add key="GroupKey-CommandKey-CircuitBreakerForcedClosed" value="false" />
<add key="GroupKey-CommandKey-CircuitBreakerErrorThresholdPercentage" value="50" />
<add key="GroupKey-CommandKey-CircuitBreakerSleepWindowInMilliseconds" value="5000" />
<add key="GroupKey-CommandKey-CircuitBreakerRequestVolumeThreshold" value="20" />
<add key="GroupKey-CommandKey-MetricsHealthSnapshotIntervalInMilliseconds" value="500" />
<add key="GroupKey-CommandKey-MetricsRollingStatisticalWindowInMilliseconds" value="10000" />
<add key="GroupKey-CommandKey-MetricsRollingStatisticalWindowBuckets" value="10" />
<add key="GroupKey-CommandKey-MetricsRollingPercentileEnabled" value="true" />
<add key="GroupKey-CommandKey-MetricsRollingPercentileWindowInMilliseconds" value="60000" />
<add key="GroupKey-CommandKey-MetricsRollingPercentileWindowBuckets" value="6" />
<add key="GroupKey-CommandKey-MetricsRollingPercentileBucketSize" value="100" />
```

But in order to be able to control the settings near realtime you're better off using the **HystrixJsonConfigConfigurationService**. It can fetch a json object from a remote url with the config.

```
<add key="HystrixCommandFactory-ConfigurationServiceImplementation" value="HystrixJsonConfigConfigurationService" />
<add key="HystrixJsonConfigConfigurationService-BaseLocation" value="http://hystrix-config.mydomain.com/" />
<add key="HystrixJsonConfigConfigurationService-LocationPattern" value="Hystrix-{0}-{1}.json" />
<add key="HystrixJsonConfigConfigurationService-PollingIntervalInMilliseconds" value="5000" />
```

Put a file with the following format at http://hystrix-config.mydomain.com/Hystrix-GroupKey-CommandKey.json:

```
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

The first time a Hystrix command is created by the factory it waits for the remote config to be fetched. After that it updates in a non-blocking way using a background thread at an interval defined by the earlier appsetting. It will also fail silently continuing to use the last known configuration.

For more info on the configuration see https://github.com/Netflix/Hystrix/wiki/configuration and https://github.com/Netflix/Hystrix/wiki/Operations on how to tune it for first use.

Visualizing metrics
--------------------------------

In order to expose the metrics of all of your Hystrix commands you can add the **HystrixStreamHandler** to your application using Hystrix.

```
<system.webServer>
    <handlers>
        ...
        <add name="HystrixStreamHandler" verb="*" path="hystrix.stream" type="Hystrix.Dotnet.HystrixStreamHandler" preCondition="integratedMode,runtimeVersionv4.0" />
        ...
    </handlers>
</add>system.webServer>
```

For both MVC and WebApi application the path hystrix.stream will be picked up by either MVC or WebApi instead of the handler. To make sure a request makes its way to the handler add the following Ignore to your **global.asax.cs**:

```
protected void Application_Start()
{
    // ignore route for hystrix.stream httphandler
    RouteTable.Routes.Ignore("hystrix.stream/{*pathInfo}");

    ...
}
```

The hystrix.stream is a **text/event-stream** that pushes the information from the server to the requester of the url. It does this at an interval defined by the following appsetting:

```
<add key="HystrixStreamHandler-PollingIntervalInMilliseconds" value="500" />
```

In order to see your Hystrix command in action spin up the following docker container locally:

```
docker run -d -p 8080:8080 travix/hystrix-dashboard
```

And then in the dashboard - at http://192.168.99.100:8080/ in case you use Kitematic - paste the following url where <mylocalip> points to the web application running on your local machine.

```
http://<mylocalip>/hystrix.stream
```

When requesting urls for your local application that hit your Hystrix command it should show up in the dashboard. For more info on how to read the dashboard, see https://github.com/Netflix/Hystrix/wiki/Dashboard

Known issues
--------------------------------

Unlike the original Hystrix implementation the current .Net implementation doesn't use a way to limit the maximum number of concurrent requests per command. Using the ExecuteAsync method will make efficient use of the threadpool, so it's not entirely clear whether it will give us any benefits.

Neither are retries implemented at this moment.
