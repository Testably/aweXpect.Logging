# aweXpect.Logging

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Logging)](https://www.nuget.org/packages/aweXpect.Logging)
[![Build](https://github.com/Testably/aweXpect.Logging/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/aweXpect.Logging/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Logging&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_aweXpect.Logging)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Logging&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_aweXpect.Logging)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FaweXpect.Logging%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/aweXpect.Logging/main)

Expectations for logging with `Microsoft.Extensions.Logging` for [aweXpect](https://github.com/Testably/aweXpect).

## At a glance

```csharp
// verify the log records of a FakeLogCollector
await Expect.That(collector).HasLogged(LogLevel.Warning)
    .WithMessage("Retry limit * reached").AsWildcard()
    .Once();
await Expect.That(collector).HasNotLogged(LogLevel.Error);
```

## Overview

| Expectation    | Subject            | Summary                                         |
|----------------|--------------------|-------------------------------------------------|
| `HasLogged`    | `FakeLogCollector` | logged a record that matches the expectation    |
| `HasNotLogged` | `FakeLogCollector` | logged no record that matches the expectation   |

## Documentation

The full documentation is available at
[docs.testably.org](https://docs.testably.org/aweXpect/extensions/aweXpect.Logging/).
