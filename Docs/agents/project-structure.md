# Project structure

## Repository layout

| Path | Content |
|------|---------|
| `Source/aweXpect.Logging/` | The only shipped package. Root namespace `aweXpect`. |
| `Tests/aweXpect.Logging.Tests/` | xUnit v2 unit tests on net8.0, net10.0 and net48. |
| `Tests/aweXpect.Logging.Api.Tests/` | NUnit public API tests (net8.0) with the expected surface per target framework in `Expected/`. |
| `Tests/Aot/aweXpect.Logging.Aot/` | Console app with smoke checks, run under the JIT and published with Native AOT (net10.0). Not a test project. |
| `Benchmarks/aweXpect.Logging.Benchmarks/` | BenchmarkDotNet project (net8.0). |
| `Pipeline/` | Fallout build pipeline (`Build.*.cs`, one file per area). Has its own `Directory.Build.*` files that do not import the root ones. |
| `Docs/pages/` | User documentation, published to docs.testably.org. `00-index.md` embeds the README through the `{README}` placeholder. |
| `Docs/agents/` | Guides for authors and agents (not published). |
| `.github/workflows/` | CI (`ci.yml`), main and release build (`build.yml`), follow-ups (`ci-analysis.yml`, `notify-docs-site.yml`). |

Shared MSBuild settings: `Directory.Build.props` (strong naming), `Source/Directory.Build.props` (target frameworks, warnings as errors, packaging, AOT flags), `Tests/Directory.Build.props` (test frameworks and packages), `Tests/Aot/Directory.Build.props` (AOT publish settings). `Tests/aweXpect.Logging.Api.Tests/Helper.cs` reads the target frameworks from `Source/Directory.Build.props`, so a new target framework gets an expected API file automatically after `AcceptApiChanges`.

## Library architecture

Everything builds on the extension points of `aweXpect.Core` (`IThat<T>`, `ExpectationBuilder`, constraints, result types and options such as `Quantifier`, `StringEqualityOptions` and `RepeatedCheckOptions`).

- `ThatFakeLogCollector.Logged.cs`: `Logged()` and `Logged(LogLevel)` on `IThat<FakeLogCollector?>`, plus the private `LoggedConstraint` that evaluates the records, and `CollectorOfLoggerConstraint`, which replaces a `FakeLogger` value by its collector.
- `ThatFakeLogCollector.DidNotLog.cs`: `DidNotLog()` and `DidNotLog(LogLevel)`. They reuse the same constraint with `Quantifier.Never()`.
- `ThatFakeLogCollector.LoggedLevels.cs`: one shortcut per log level (`LoggedWarning()`, `DidNotLogError()`, ...) that delegates to the `LogLevel` overload and returns the same result type.
- `ThatFakeLogger.cs`: the same entry points on `IThat<FakeLogger?>`. `CollectorOfLogger` registers the expectations on the collector with the expectation of the logger, so the subject stays the logger.
- `Options/LogRecordFilter.cs` (internal): the conditions a record must meet (level, `OrHigher`, category, event ID, exception, message, message template, properties). It describes itself for the expectation text and is shared by the main expectation and each `Then` step.
- `Results/`: the fluent result types. `LogRecordFilterResult<TSelf>` holds the filters without options (`WithCategory<T>()`, `WithEventId`, `WithException<T>()`, `WithMessageTemplate`); `LoggedResult` and `DidNotLogResult` derive from it. A method that adds options returns a more specific result that derives from the result of its side, so the other filters stay available:
  - `*LevelResult` after a log level (adds `OrHigher`).
  - `*MessageResult`, `*CategoryResult` after `WithMessage` / `WithCategory(string)`, taking the Core string options (`AsWildcard`, `AsRegex`, `AsPrefix`, `IgnoringCase`, ...).
  - `*PropertyResult` after `WithProperty`, `*PropertyValueResult` after its `WithValue`.
  - `LoggedCountResult` after a quantifier (`Once()`, `AtLeast(...)`, ...), `LoggedInOrderResult` after `Then`.
  - `*WithinResult` after `Within`, the only place that offers `CheckEvery`.
  - `LogRecordStep` and `LogRecordStepResult` (with its `LogRecordStep*Result` variants) are the builders inside `Then(r => r.LoggedWarning()...)`; they mirror the filters of `LoggedResult`.

Result constructors are `internal`, so a result can take the options of further filters without a breaking change, and results are only `sealed` where nothing derives from them.

## Where to add things

| Change | Files |
|--------|-------|
| New filter on records | `LogRecordFilter` (matching and description), the results of each side that should offer it (`Logged*`, `DidNotLog*`, `LogRecordStep*`), unit tests in a new `ThatFakeLogCollector.Logged.With<Name>Tests.cs`, an AOT check, `Docs/pages/01-log-records.md`, accepted API files. |
| New entry point | `ThatFakeLogCollector.*.cs` and its mirror in `ThatFakeLogger.cs` (plus `LogRecordStep` if it fits inside `Then`), README overview table, docs, API files. |
| Failure message wording | `LoggedConstraint` / `LogRecordFilter`, then every test, AOT check and doc sample that pins the text. |
| New dependency or Core floor bump | `Directory.Packages.props`; explain the consumer impact in the PR. |
