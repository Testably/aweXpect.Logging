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
but it did not log any such record

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
considered nor listed, unless you specify a [timeout](#timeout).

With `OrHigher()`, a record with a more severe level also counts, e.g. to verify that no warning or error was logged:

```csharp
await Expect.That(collector).HasLogged(LogLevel.Warning).OrHigher().Never();
```

Without a log level, a record of any level is enough:

```csharp
await Expect.That(collector).HasLogged();
```

## Message

You can additionally verify the message of the record:

```csharp
logger.LogWarning("Retry limit 3 reached");

await Expect.That(collector).HasLogged(LogLevel.Warning).WithMessage("Retry limit 3 reached");
await Expect.That(collector).HasLogged().WithMessage("Retry limit * reached").AsWildcard();
```

The message must be equal by default. As for other strings, you can change the match type with `AsWildcard()`,
`AsRegex()`, `AsPrefix()` or `AsSuffix()`, and the comparison e.g. with `IgnoringCase()`. If the message differs,
the expectation would fail with:

```text title="Failure message"
Expected that collector
has logged a record with level Warning and message matching "Retry limit * reached" at least once,
but it did not log any such record

Records:
[
  [Warning] "Connection lost"
]
```

If `WithMessage` is specified more than once, the message of a record must match all of them.

## Exception

You can additionally verify the type of the exception of the record, which includes derived types:

```csharp
logger.LogError(new FileNotFoundException(), "Configuration missing");

await Expect.That(collector).HasLogged(LogLevel.Error).WithException<IOException>();
```

The failure message lists the type of the exception after the message of a record, e.g. if the exception had another
type:

```text title="Failure message"
Expected that collector
has logged a record with level Error and exception of type IOException at least once,
but it did not log any such record

Records:
[
  [Error] "Configuration missing" (InvalidOperationException)
]
```

## Category and event ID

When several loggers write to the same collector, e.g. with `AddFakeLogging()` in a service collection, you can verify
the category of the logger and the event ID of the record:

```csharp
await Expect.That(collector).HasLogged(LogLevel.Warning).WithCategory<OrderService>();
await Expect.That(collector).HasLogged().WithCategory("MyApp.Orders").WithEventId(1001);
```

`WithCategory<T>()` expects the category of an `ILogger<T>`, i.e. the full name of the type, where nested types are
separated by a dot and generic types are named without their type arguments. The category and the event ID are
compared exactly, and the failure message lists them for each record when they are compared:

```text title="Failure message"
Expected that collector
has logged a record with category "MyApp.Orders" and event ID 1001 at least once,
but it did not log any such record

Records:
[
  [Warning] "Retry limit reached" (category "MyApp.Orders", event ID 1002)
]
```

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

As for quantified collection expectations in aweXpect, the failure message additionally lists the records that explain
the failure, when the quantifier fails in only one direction:

- with only an upper bound, e.g. `AtMost(…)` or `Never()`, the matching records:
  ```text title="Failure message"
  Expected that collector
  has not logged a record with level Warning,
  but it logged 1 such record

  Matching records:
  [
    [Warning] "Retry 1"
  ]

  Records:
  [
    [Information] "Starting",
    [Warning] "Retry 1"
  ]
  ```
- with only a lower bound above one, e.g. `AtLeast(2.Times())`, the not matching records.

When the expectation is negated, this depends on its negation instead, e.g. `AtLeast(2.Times())` lists the matching
records when negated.

Only one quantifier can be specified for each `HasLogged`, after the message:

```csharp
await Expect.That(collector).HasLogged(LogLevel.Warning).WithMessage("Retry *").AsWildcard().Twice();
```

## Not logged

You can verify that no matching record was logged:

```csharp
await Expect.That(collector).HasNotLogged(LogLevel.Error);
await Expect.That(collector).HasNotLogged(LogLevel.Warning).OrHigher();
await Expect.That(collector).HasNotLogged().WithMessage("*password*").AsWildcard();
```

`HasNotLogged` takes the same filters as `HasLogged`, but no quantifier, as it is the same as `HasLogged(…).Never()`.
If an error was logged, `HasNotLogged(LogLevel.Warning).OrHigher()` would fail with:

```text title="Failure message"
Expected that collector
has not logged a record with level Warning or higher,
but it logged 1 such record

Matching records:
[
  [Error] "Retry limit reached"
]

Records:
[
  [Information] "Starting",
  [Error] "Retry limit reached"
]
```

## Timeout

Records of a background service, e.g. a hosted service or a timer, can be logged after the expectation is evaluated.
With `Within`, the records are read repeatedly until the timeout:

```csharp
await Expect.That(collector).HasLogged(LogLevel.Information).WithMessage("Started").Within(TimeSpan.FromSeconds(5));
await Expect.That(collector).HasLogged(LogLevel.Warning).Once().Within(TimeSpan.FromSeconds(5));
await Expect.That(collector).HasNotLogged(LogLevel.Error).OrHigher().Within(TimeSpan.FromSeconds(5));
```

The records are read until their amount decides the outcome, so the expectation only waits as long as necessary:

- An expectation that further records can meet, e.g. `HasLogged` or `AtLeast(…)`, succeeds as soon as it is met.
- An expectation that further records can violate, e.g. `HasNotLogged`, `Never()`, `AtMost(…)` or `Once()`, waits for
  the full timeout and fails as soon as it is violated.

The failure message then lists the records that were logged until the expectation failed:

```text title="Failure message"
Expected that collector
has not logged a record with level Error or higher within 0:05,
but it logged 1 such record

Matching records:
[
  [Error] "Retry limit reached"
]

Records:
[
  [Information] "Started",
  [Error] "Retry limit reached"
]
```

When the evaluation is canceled before the outcome is decided, e.g. via `WithCancellation`, the expectation is
inconclusive.
