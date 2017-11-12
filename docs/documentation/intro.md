# How to use

The circuit breakers are identifyable by group and command key. To make sure you get the same Hystrix command object for each group and command key combination you should use the factory the retrieve the command.

## Creating the factory manually

```csharp
var options = HystrixOptions.CreateDefault();
var hystrixCommandFactory = new HystrixCommandFactory(options);
var hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
```

## Creating the factory in ASP.NET

In ASP.NET we can use the `AspNetHystrixCommandFactoryHelper` helper class to create our factory, which will automatically pick up the configuration from the web.config.

```csharp
var helper = new AspNetHystrixCommandFactoryHelper();
var factory = helper.CreateFactory();
var hystrixCommand = hystrixCommandFactory.GetHystrixCommand("groupKey", "commandKey");
```

## Creating the factory in ASP.NET Core

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

## Setting up fallbacks

A fallback can be defined as a second lambda expression right after the encapsulated command. This way e.g. in case of a failing network connection the fallback can return some cached or default value.

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
