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
has logged a record with level Warning,
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
has logged a record with level Warning,
but it did not log any record
```

The records are read once, when the expectation is evaluated, so a record that is logged afterwards is neither
considered nor listed.
