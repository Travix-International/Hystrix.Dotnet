# Hystrix.Dotnet

A combination of circuit breaker and timeout. The .NET version of the open source [Hystrix library](https://github.com/Netflix/Hystrix) built by Netflix.

[![Build Status .NET](https://ci.appveyor.com/api/projects/status/1a09t29hcvfo1bbq/branch/master?svg=true)](https://ci.appveyor.com/project/Travix-International/Hystrix.Dotnet/)
[![Version](https://img.shields.io/nuget/v/Hystrix.Dotnet.svg)](https://www.nuget.org/packages/Hystrix.Dotnet)
[![Coverage Status](https://coveralls.io/repos/github/Travix-International/Hystrix.Dotnet/badge.svg?branch=upgrade-to-net-core)](https://coveralls.io/github/Travix-International/Hystrix.Dotnet?branch=upgrade-to-net-core)
[![License](https://img.shields.io/github/license/Travix-International/Hystrix.Dotnet.svg)](https://github.com/Travix-International/Hystrix.Dotnet/blob/master/LICENSE)

## Why?

In order to isolate failure in one dependency from taking down another component. Whenever the circuit breaker opens it returns an exception or runs the fallback without burdening the failing system. It sends through a single request on a regular interval to see if the dependent system is back in business.

## How to use?

See the details and examples in the [Documentation](documentation/intro.md), and in the [API Reference](api/index.md).