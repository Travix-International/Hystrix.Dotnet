# Visualizing metrics

In order to expose the metrics of all of your Hystrix commands, we need to publish them in our web application. In ASP.NET we have to use a handler, while in ASP.NET Core we should add a middleware to our pipeline.

## ASP.NET

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

## ASP.NET Core

In ASP.NET Core we have to add a middleware to our pipeline to publish the metrics. We can do this in the `Configure` method of our `Startup` class.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    ...
    app.UseHystrixMetricsEndpoint("hystrix.stream");
}
```

## The metrics stream

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