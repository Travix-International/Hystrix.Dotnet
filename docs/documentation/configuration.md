# Configuration

All the configuration parameters are controller via an `IOptions<HystrixOptions>` object, which is passed in to the `HystrixCommandFactory`.
There are two main modes of configuring our individual commands, based on the value of the `ConfigurationServiceImplementation` property.
 - If the value is **HystrixLocalConfigConfigurationService**, then the configuration is read from the `LocalOptions` property, which is populated from the configuration deployed with the application. (in ASP.NET it is coming from the web.config, while  in ASP.NET Core, it's typically in an appsettings.json file.)
 - If the value is **HystrixJsonConfigConfigurationService**, then the configuration is retrieved from an external service, so that it can by dynamically changed in near real time.

 ## Using local (deployed) configuration

 ### ASP.NET

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
        <defaultConfiguration commandTimeoutInMilliseconds="1500" />
      </localOptions>
    </hystrix>
  </hystrix.dotnet>
```

This way we can add multiple groups and commands, and fine-tune each independently.
The above values are also the defaults if we omit any of the attributes. (Or if we don't add any configuration to the web.config.)

In the `defaultConfiguration` element we can adjust the "default" configuration, which is used for commands which don't have specific configuration specified for them.

### ASP.NET Core

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
            },
            "DefaultOptions": {
                "CommandTimeoutInMilliseconds": 1500
            }
        }
    }
}
```

In the `DefaultOptions` field we can adjust the "default" configuration, which is used for commands which don't have specific configuration specified for them.

Then set up the options object in our DI configuration, in the `ConfigureServices` method of the `Startup` class.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.Configure<HystrixOptions>(options => Configuration.GetSection("Hystrix").Bind(options));
}
```

## Using external Json Configuration

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