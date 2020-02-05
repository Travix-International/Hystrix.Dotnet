# Using Hystrix.Dotnet in ASP.NET

In order to use Hystrix.Dotnet in ASP.NET we need to do the following steps.

Add a dependency to the package `Hystrix.Dotnet.AspNet` using the Manage NuGet Packages window, or by using the Package Manager Console and issue the following command.

```
Install-Package Hystrix.Dotnet.AspNet
```

(Optional) Add our custom configuration to the `web.config`.

```xml
  <configSections>
    <sectionGroup name="hystrix.dotnet">
      <section name="hystrix" type="Hystrix.Dotnet.AspNet.HystrixConfigSection, Hystrix.Dotnet.AspNet" />
    </sectionGroup>
  </configSections>

  <hystrix.dotnet>
    <hystrix serviceImplementation="HystrixLocalConfigurationService" metricsStreamPollIntervalInMilliseconds="2000">
      <localOptions>
        <commandGroups>
          <add key="TestGroup">
            <commands>
              <add key="TestCommand" commandTimeoutInMilliseconds="1250" />
            </commands>
          </add>
        </commandGroups>
        <defaultConfiguration commandTimeoutInMilliseconds="1500" />
      </localOptions>
    </hystrix>
  </hystrix.dotnet>
```

(Optional) Set up the metrics stream endpoint in the web.config.

```xml
  <system.webServer>
    <handlers>
      ...
      <add name="HystrixStreamHandler" verb="*" path="hystrix.stream" type="Hystrix.Dotnet.AspNet.HystrixStreamHandler" preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
  </system.webServer>
```

Then in our controller we can use the `AspNetHystrixCommandFactoryHelper` to instantiate the command factory, and get the reference to a particular command.

```csharp
public class HelloController : ApiController
{
    private readonly IHystrixCommand testHystrixCommand;

    public HelloController()
    {
        var helper = new AspNetHystrixCommandFactoryHelper();

        var factory = helper.CreateFactory();

        testHystrixCommand = factory.GetHystrixCommand("TestGroup", "TestCommand");
    }
}
```