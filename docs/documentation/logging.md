# Logging

Hystrix.Dotnet logs some diagnostic information using LibLog. You can hook these log messages into logging libraries like log4net, NLog or Serilog, or you can provide your own logging provider implementation.

You can find an example of setting the logging up for Serilog in the [ASP.NET Core sample](/samples/Hystrix.Dotnet.Samples.AspNetCore), and you can find examples for other logging libraries in the [LibLog project](https://github.com/damianh/LibLog).