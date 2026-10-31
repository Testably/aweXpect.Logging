# aweXpect.Logging

[![Nuget](https://img.shields.io/nuget/v/aweXpect.Logging)](https://www.nuget.org/packages/aweXpect.Logging)
[![Build](https://github.com/Testably/aweXpect.Logging/actions/workflows/build.yml/badge.svg)](https://github.com/Testably/aweXpect.Logging/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Logging&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Testably_aweXpect.Logging)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Testably_aweXpect.Logging&metric=coverage)](https://sonarcloud.io/summary/overall?id=Testably_aweXpect.Logging)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FTestably%2FaweXpect.Logging%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/Testably/aweXpect.Logging/main)

Expectations for logging with `Microsoft.Extensions.Logging` for [aweXpect](https://github.com/Testably/aweXpect).

## At a glance

```csharp
// collect the log records with a FakeLogger from Microsoft.Extensions.Diagnostics.Testing
FakeLogCollector collector = new();
ILogger logger = new FakeLogger(collector);
logger.LogWarning("Retry limit reached");

// verify that a record with the log level was logged
await Expect.That(collector).LoggedWarning();
```

A failing expectation lists the collected records. If only an error was logged, `LoggedWarning()` would
fail with:

```text title="Failure message"
Expected that collector
logged a record with level Warning at least once,
but it did not log any such record

Records:
[
  [Error] "Retry limit reached"
]
```

## Overview

| Expectation                                            | Subject                          | Summary                                                                 |
|--------------------------------------------------------|----------------------------------|-------------------------------------------------------------------------|
| [`Logged`](https://docs.testably.org/aweXpect/extensions/aweXpect.Logging/log-records#level)         | `FakeLogCollector`, `FakeLogger` | logged at least one record with the log level, e.g. `LoggedWarning()`   |
| [`DidNotLog`](https://docs.testably.org/aweXpect/extensions/aweXpect.Logging/log-records#not-logged) | `FakeLogCollector`, `FakeLogger` | logged no record with the log level, e.g. `DidNotLogError()`            |

## Documentation

The full documentation is available at
[docs.testably.org](https://docs.testably.org/aweXpect/extensions/aweXpect.Logging/):

- [Log records](https://docs.testably.org/aweXpect/extensions/aweXpect.Logging/log-records): `Logged` and `DidNotLog` on the records of a `FakeLogCollector`
