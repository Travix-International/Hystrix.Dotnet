# How to use

Circuit breakers are the main concept in Hystrix, and they are identifyable by a group and command key, which are arbitrary strings to support structuring and organizing the various circuit breakers we have in our application. Every circuit breaker can have its own configuration regarding its timeout, fallback mechanism, error threshold, etc.

Once we have a reference to a circuit breaker (represented by the `IHystrixCommand` interface), we can execute an operation through it using either the synchronous version:

```csharp
T result = hystrixCommand.Execute<T>(() => myFunctionWithReturnTypeT());
```

Or use the async version:

```csharp
T result = await hystrixCommand.ExecuteAsync<T>(() => myAsyncFunctionWithReturnTypeTaskT());
```

## Accessing commands

Getting a reference to a particular command happens through a factory object. Creating the factory can happen differently depending on what kind of environment we are in.

### Creating the factory manually

We can always create a factory manually. This is only recommended if we don't use any dependency injection, for example it can be useful in a console application.

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