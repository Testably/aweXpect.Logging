import PackageInstall from '@site/src/components/PackageInstall';

# aweXpect.Logging

[aweXpect.Logging](https://github.com/Testably/aweXpect.Logging) contains expectations for the log records that are
written with `Microsoft.Extensions.Logging`. It is an extension for [aweXpect](/aweXpect).

## Installation

<PackageInstall package="aweXpect.Logging" />

The package adds its expectations to aweXpect, so the test project also needs the
[`aweXpect`](/aweXpect/getting-started) package.

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

| Expectation                                         | Subject                          | Summary                                                               |
|-----------------------------------------------------|----------------------------------|-----------------------------------------------------------------------|
| [`Logged`](./01-log-records.md#level)               | `FakeLogCollector`, `FakeLogger` | logged at least one record with the log level, e.g. `LoggedWarning()` |
| [`DidNotLog`](./01-log-records.md#not-logged)       | `FakeLogCollector`, `FakeLogger` | logged no record with the log level, e.g. `DidNotLogError()`          |
