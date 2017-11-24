# Hystrix.Dotnet

A combination of circuit breaker and timeout. The .NET version of the open source [Hystrix library](https://github.com/Netflix/Hystrix) built by Netflix.

[![Build Status .NET](https://ci.appveyor.com/api/projects/status/1a09t29hcvfo1bbq/branch/master?svg=true)](https://ci.appveyor.com/project/Travix-International/Hystrix.Dotnet/)
[![Version](https://img.shields.io/nuget/v/Hystrix.Dotnet.svg)](https://www.nuget.org/packages/Hystrix.Dotnet)
[![Coverage Status](https://coveralls.io/repos/github/Travix-International/Hystrix.Dotnet/badge.svg?branch=upgrade-to-net-core)](https://coveralls.io/github/Travix-International/Hystrix.Dotnet?branch=upgrade-to-net-core)
[![License](https://img.shields.io/github/license/Travix-International/Hystrix.Dotnet.svg)](https://github.com/Travix-International/Hystrix.Dotnet/blob/master/LICENSE)

## Why?

In order to isolate failure in one dependency from taking down another component. Whenever the circuit breaker opens it returns an exception or runs the fallback without burdening the failing system. It sends through a single request on a regular interval to see if the dependent system is back in business.

## How to use

Circuit breakers are the main concept in Hystrix, and they are identifyable by a group and command key, which are arbitrary strings to support structuring and organizing the various circuit breakers we have in our application. Every circuit breaker can have its own configuration regarding its timeout, fallback mechanism, error threshold, etc.

Once we have a reference to a circuit breaker (represented by the `IHystrixCommand` interface), we can execute an operation through it using either the synchronous version:

```csharp
T result = hystrixCommand.Execute<T>(() => myFunctionWithReturnTypeT());
```

Or use the async version:

```csharp
T result = await hystrixCommand.ExecuteAsync<T>(() => myAsyncFunctionWithReturnTypeTaskT());
```

In the [documentation](https://travix-international.github.io/Hystrix.Dotnet/documentation/intro.html) you can find more examples, and details about creating commands and customizing the configuration.

## Sample projects

In the [samples](/samples) directory you can find an example project illustrating the configuration of Hystrix for [ASP.NET](/samples/Hystrix.Dotnet.Samples.AspNet) and [ASP.NET Core](/samples/Hystrix.Dotnet.Samples.AspNetCore).

## Known issues

Unlike the original Hystrix implementation, the current .Net implementation doesn't use a way to limit the maximum number of concurrent requests per command. Using the ExecuteAsync method will make efficient use of the threadpool, so it's not entirely clear whether it will give us any benefits.

Neither are retries implemented at this moment.
