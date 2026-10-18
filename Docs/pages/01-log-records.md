# Log records

The expectations verify the records of a
[`FakeLogCollector`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.testing.fakelogcollector)
from the `Microsoft.Extensions.Diagnostics.Testing` package, which collects everything that is logged to a
[`FakeLogger`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.testing.fakelogger):

```csharp
FakeLogCollector collector = new();
ILogger logger = new FakeLogger(collector);
```

All expectations are also available on a `FakeLogger` itself, which verifies the records of its `Collector`, so also
the records of other loggers that write to the same collector:

```csharp
FakeLogger<OrderService> logger = new();
logger.LogWarning("Retry limit reached");

await Expect.That(logger).LoggedWarning().And.DidNotLogError();
```

`And` and `Or` then continue with further expectations on these records, e.g. `Logged` or `DidNotLog`.

## Level

You can verify that at least one record with the log level was logged:

```csharp
logger.LogWarning("Retry limit reached");

await Expect.That(collector).LoggedWarning();
```

There is a method for each log level: `LoggedTrace()`, `LoggedDebug()`, `LoggedInformation()`, `LoggedWarning()`,
`LoggedError()` and `LoggedCritical()`. When the level is a parameter, e.g. in a theory, you can pass it instead:

```csharp
await Expect.That(collector).Logged(level);
```

The log level must match exactly, so a record with the level `Error` does not satisfy `LoggedWarning()`. If only an
error was logged, the expectation would fail with:

```text title="Failure message"
Expected that collector
logged a record with level Warning at least once,
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
logged a record with level Warning at least once,
but it did not log any record
```

The records are read once, when the expectation is evaluated, so a record that is logged afterwards is neither
considered nor listed, unless you specify a [timeout](#timeout).

With `OrHigher()`, a record with a more severe level also counts, e.g. to verify that no warning or error was logged:

```csharp
await Expect.That(collector).LoggedWarning().OrHigher().Never();
```

Without a log level, a record of any level is enough:

```csharp
await Expect.That(collector).Logged();
```

## Message

You can additionally verify the message of the record:

```csharp
logger.LogWarning("Retry limit 3 reached");

await Expect.That(collector).LoggedWarning().WithMessage("Retry limit 3 reached");
await Expect.That(collector).Logged().WithMessage("Retry limit * reached").AsWildcard();
```

The message must be equal by default. As for other strings, you can change the match type with `AsWildcard()`,
`AsRegex()`, `AsPrefix()` or `AsSuffix()`, and the comparison e.g. with `IgnoringCase()`. If the message differs,
the expectation would fail with:

```text title="Failure message"
Expected that collector
logged a record with level Warning and message matching "Retry limit * reached" at least once,
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

await Expect.That(collector).LoggedError().WithException<IOException>();
```

The failure message lists the type of the exception after the message of a record, e.g. if the exception had another
type:

```text title="Failure message"
Expected that collector
logged a record with level Error and exception of type IOException at least once,
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
await Expect.That(collector).LoggedWarning().WithCategory<OrderService>();
await Expect.That(collector).Logged().WithCategory("MyApp.Orders").WithEventId(1001);
await Expect.That(collector).Logged().WithCategory("MyApp.Orders.").AsPrefix();
```

`WithCategory<T>()` expects the category of an `ILogger<T>`, i.e. the full name of the type, where nested types are
separated by a dot and generic types are named without their type arguments, and compares it exactly. The category of
`WithCategory(string)` must be equal by default, but as for the message, you can change the match type, e.g. with
`AsPrefix()` or `AsWildcard()`, and the comparison, e.g. with `IgnoringCase()`. The event ID is compared exactly, and
the failure message lists the category and the event ID for each record when they are compared:

```text title="Failure message"
Expected that collector
logged a record with category "MyApp.Orders" and event ID 1001 at least once,
but it did not log any such record

Records:
[
  [Warning] "Retry limit reached" (category "MyApp.Orders", event ID 1002)
]
```

## Properties and message template

With structured logging, a record stores its message template and the values of its placeholders as properties:

```csharp
logger.LogWarning("Retry {Attempt} of {MaxAttempts}", 3, 5);

await Expect.That(collector).LoggedWarning().WithProperty("Attempt");
await Expect.That(collector).LoggedWarning().WithProperty("Attempt").WithValue("3");
await Expect.That(collector).Logged().WithMessageTemplate("Retry {Attempt} of {MaxAttempts}").Twice();
```

`WithProperty` alone only requires the property, while `WithValue` also verifies its value. The `FakeLogger` stores the
values of the properties as strings in the invariant culture, e.g. `"1.5"` for the `double` 1.5, so the expected value
is a string, too. It must be equal by default, and as for the message, you can change the match type of the value,
e.g. with `AsWildcard()`, and the comparison, e.g. with `IgnoringCase()`:

```csharp
await Expect.That(collector).Logged().WithProperty("OrderId").WithValue("ORD-*").AsWildcard();
```

The template does not depend on the logged values, so it identifies the statement that logged a record. It must be
equal by default, and as for the message, you can change the match type, e.g. with `AsPrefix()` or `AsWildcard()`, and
the comparison, e.g. with `IgnoringCase()`:

```csharp
await Expect.That(collector).Logged().WithMessageTemplate("Retry {Attempt} of {MaxAttempts}*").AsWildcard();
```

The failure message lists the compared template and properties for each record:

```text title="Failure message"
Expected that collector
logged a record with level Warning and property Attempt with value equal to "3" at least once,
but it did not log any such record

Records:
[
  [Warning] "Retry 2 of 5" (property Attempt "2")
]
```

A record that was logged with a custom state object instead of a message template has no properties, which the failure
message shows as `(no structured state)`.

## Quantifiers

By default, a single matching record is enough. You can specify how many matching records are expected instead:

```csharp
await Expect.That(collector).LoggedWarning().Once();
await Expect.That(collector).LoggedWarning().Twice();
await Expect.That(collector).LoggedWarning().Exactly(3.Times());
await Expect.That(collector).LoggedWarning().AtLeast(2.Times());
await Expect.That(collector).LoggedWarning().AtMost().Once();
await Expect.That(collector).LoggedWarning().Between(1).And(3.Times());
await Expect.That(collector).LoggedWarning().LessThan(3.Times());
await Expect.That(collector).LoggedWarning().MoreThan().Once();
await Expect.That(collector).LoggedError().Never();
```

If two warnings were logged, `LoggedWarning().Once()` would fail with:

```text title="Failure message"
Expected that collector
logged a record with level Warning exactly once,
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
  did not log a record with level Warning,
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

Only one quantifier can be specified for each `Logged`, after the message:

```csharp
await Expect.That(collector).LoggedWarning().WithMessage("Retry *").AsWildcard().Twice();
```

## Not logged

You can verify that no matching record was logged:

```csharp
await Expect.That(collector).DidNotLogError();
await Expect.That(collector).DidNotLogWarning().OrHigher();
await Expect.That(collector).DidNotLog().WithMessage("*password*").AsWildcard();
```

As for `Logged`, there is a method for each log level, e.g. `DidNotLogError()`, and `DidNotLog(LogLevel)` for a level
that is a parameter. `DidNotLog` takes the same filters as `Logged`, but no quantifier, as it is the same as `Logged(…).Never()`.
If an error was logged, `DidNotLogWarning().OrHigher()` would fail with:

```text title="Failure message"
Expected that collector
did not log a record with level Warning or higher,
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

## Order

With `Then`, you can verify that further records were logged after a matching record, in the given order:

```csharp
await Expect.That(collector).LoggedInformation().WithMessage("Starting")
    .Then(
        r => r.LoggedWarning().WithMessage("Retry *").AsWildcard(),
        r => r.LoggedError().OrHigher().WithException<TimeoutException>());
```

Each step takes the same filters as `Logged` and must match a record that was logged after the one of the previous
step, so a record cannot match two steps, while other records may be logged in between. As the order implies that each
record was logged at least once, it cannot be combined with a quantifier. If a record was logged too early or not at
all, the expectation would fail with:

```text title="Failure message"
Expected that collector
logged a record with level Information and message equal to "Starting", then
a record with level Warning and message matching "Retry *", then
a record with level Error or higher and exception of type TimeoutException in order,
but it logged a record with level Warning and message matching "Retry *" too early

Records:
[
  [Warning] "Retry 1",
  [Information] "Starting"
]
```

## Timeout

Records of a background service, e.g. a hosted service or a timer, can be logged after the expectation is evaluated.
With `Within`, the records are read repeatedly until the timeout:

```csharp
await Expect.That(collector).LoggedInformation().WithMessage("Started").Within(TimeSpan.FromSeconds(5));
await Expect.That(collector).LoggedWarning().Once().Within(TimeSpan.FromSeconds(5));
await Expect.That(collector).DidNotLogError().OrHigher().Within(TimeSpan.FromSeconds(5));
```

The records are read until their amount decides the outcome, so the expectation only waits as long as necessary:

- An expectation that further records can meet, e.g. `Logged` or `AtLeast(…)`, succeeds as soon as it is met.
- An expectation that further records can violate, e.g. `DidNotLog`, `Never()`, `AtMost(…)` or `Once()`, waits for
  the full timeout and fails as soon as it is violated.

The records are read every 100 milliseconds by default (`Customize.aweXpect.Settings().DefaultCheckInterval`). You can
change the interval with `CheckEvery` directly after `Within`:

```csharp
await Expect.That(collector).LoggedWarning()
    .Within(TimeSpan.FromSeconds(5)).CheckEvery(TimeSpan.FromMilliseconds(10));
```

The failure message then lists the records that were logged until the expectation failed:

```text title="Failure message"
Expected that collector
did not log a record with level Error or higher within 0:05,
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
