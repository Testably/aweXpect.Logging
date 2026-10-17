# Log records

The expectations verify the records of a
[`FakeLogCollector`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.testing.fakelogcollector)
from the `Microsoft.Extensions.Diagnostics.Testing` package, which collects everything that is logged to a
[`FakeLogger`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.testing.fakelogger):

```csharp
FakeLogCollector collector = new();
ILogger logger = new FakeLogger(collector);
```

## Level

You can verify that at least one record with the log level was logged:

```csharp
logger.LogWarning("Retry limit reached");

await Expect.That(collector).HasLogged(LogLevel.Warning);
```

The log level must match exactly, so a record with the level `Error` does not satisfy
`HasLogged(LogLevel.Warning)`. If only an error was logged, the expectation would fail with:

```text title="Failure message"
Expected that collector
has logged a record with level Warning at least once,
but it only logged records with other levels

Records:
[
  [Error] "Retry limit reached"
]
```

The failure message lists the level and the message of the first records. When nothing was logged at all, it says so
instead:

```text title="Failure message"
Expected that collector
has logged a record with level Warning at least once,
but it did not log any record
```

The records are read once, when the expectation is evaluated, so a record that is logged afterwards is neither
considered nor listed.

## Quantifiers

By default, a single matching record is enough. You can specify how many matching records are expected instead:

```csharp
await Expect.That(collector).HasLogged(LogLevel.Warning).Once();
await Expect.That(collector).HasLogged(LogLevel.Warning).Twice();
await Expect.That(collector).HasLogged(LogLevel.Warning).Exactly(3.Times());
await Expect.That(collector).HasLogged(LogLevel.Warning).AtLeast(2.Times());
await Expect.That(collector).HasLogged(LogLevel.Warning).AtMost().Once();
await Expect.That(collector).HasLogged(LogLevel.Warning).Between(1).And(3.Times());
await Expect.That(collector).HasLogged(LogLevel.Warning).LessThan(3.Times());
await Expect.That(collector).HasLogged(LogLevel.Warning).MoreThan().Once();
await Expect.That(collector).HasLogged(LogLevel.Error).Never();
```

If two warnings were logged, `HasLogged(LogLevel.Warning).Once()` would fail with:

```text title="Failure message"
Expected that collector
has logged a record with level Warning exactly once,
but it logged 2 such records

Records:
[
  [Warning] "Retry 1",
  [Warning] "Retry 2"
]
```

Only one quantifier can be specified for each `HasLogged`.
