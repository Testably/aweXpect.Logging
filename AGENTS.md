# aweXpect.Logging Agent Guide

Expectations for the records of a `FakeLogCollector` or `FakeLogger` (from `Microsoft.Extensions.Diagnostics.Testing`) as an extension of [aweXpect](https://github.com/Testably/aweXpect). The repository ships one package, `Source/aweXpect.Logging`.

## Setup and build

- The SDK is pinned in `global.json` (roll forward to the latest patch). Tests also need the .NET 8 runtime; the net48 tests run on Windows only.
- Package versions live in `Directory.Packages.props` (central package management); never put a version into a `.csproj`.
- `Source/Directory.Build.props` sets the library targets `netstandard2.0;net8.0;net10.0`, `LangVersion` latest, nullable (annotations only on netstandard2.0), `TreatWarningsAsErrors` (except CS1591), XML documentation, package on build and strong naming with `strongname.snk`.
- `dotnet build aweXpect.Logging.slnx` builds the library, tests, AOT smoke project and benchmarks. The Fallout pipeline in `Pipeline/` is excluded from the solution build and runs through `build.ps1` / `build.sh`.

## Design goals

Correctness is a must. The other goals are strong defaults and win in this order when they conflict: stable and compatible, AOT and trimming friendly, a consistent and pleasant API, a fast happy path, extensible, errors at compile time, documented. [Docs/agents/design-goals.md](Docs/agents/design-goals.md) explains each goal, the code rules, the checks before a commit and the goals of this repository.

## Project constraints

- **Dependency floor**: the library references only `aweXpect.Core` and `Microsoft.Extensions.Diagnostics.Testing`. Never reference the `aweXpect` package from `Source/`; it is for tests, the AOT smoke project and benchmarks. The `aweXpect.Core` version in `Directory.Packages.props` is the minimum every consumer gets, so raise it only when a new Core API is needed and say so in the PR.
- **Public API**: every public member needs XML docs. Entry points are extension methods on `IThat<FakeLogCollector?>` in `ThatFakeLogCollector.*.cs`, mirrored on `IThat<FakeLogger?>` in `ThatFakeLogger.cs`, and marked `[GuaranteesNotNull]`. Result constructors stay `internal`, so later filters are not breaking changes. Any surface change shows up in the `*.awexpect-snapshot.txt` files of `Tests/aweXpect.Logging.Api.Tests` and is reviewed there.
- **AOT and trimming**: the library is `IsTrimmable` and `IsAotCompatible` from net8.0; the analyzers only run for net10.0, because the .NET 8 analyzers predate feature guards. Do not add unguarded reflection. The AOT smoke project treats every trimmer or ILC warning as an error.
- **Language features**: the library compiles for netstandard2.0 and the tests run on net48, with only the `Nullable` and `IsExternalInit` polyfills. Features that need newer runtime types (e.g. `System.Index`/`System.Range`, so `^1`, `..` slices and list patterns) do not compile there. Build all target frameworks before pushing.
- **Failure messages are the product**: tests pin the full message with `.WithMessage("""...""")`, and the docs show the same messages.

## Code conventions

- `.editorconfig`: tabs, explicit types instead of `var`, CRLF in the working tree. Git stores LF (`* text=auto`).
- Some `.cs` files have a UTF-8 BOM, most do not; keep each file as it is. PowerShell rewrites (`Set-Content`, `Out-File`) and `sed -i` change the BOM or line endings, and the multi-line raw string literals in the tests are sensitive to that. Use an edit tool and check `git diff` for a stray U+FEFF.
- No em-dashes in docs, XML docs or comments. Keep comments rare and about the why; XML `<remarks>` are preferred.
- Tests use TUnit and nested partial classes, one file each: `ThatFakeLogCollector.Logged.WithCategoryTests.cs` holds `ThatFakeLogCollector.Logged.WithCategoryTests`. Methods are named `Member_WhenCondition_ShouldOutcome` and sorted alphabetically. Put reasons into `.Because(...)`, not comments.

## Validation

- All tests use TUnit on the Microsoft.Testing.Platform, which `global.json` opts into, so `dotnet test` runs every test project and target framework.
- Unit tests: `dotnet test --project Tests/aweXpect.Logging.Tests` (net8.0, net10.0 and, on Windows, net48). Narrow with `-f net10.0 --treenode-filter "/*/*/*WithCategoryTests/*"` while iterating.
- Public API: `dotnet test --project Tests/aweXpect.Logging.Api.Tests`. For an intentional change, run the explicit test with `dotnet test --project Tests/aweXpect.Logging.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"` and commit the updated `*.awexpect-snapshot.txt` files, never the `*.awexpect-snapshot.pending.*` ones.
- AOT: `dotnet run --project Tests/Aot/aweXpect.Logging.Aot` runs the checks under the JIT; `./build.ps1 AotSmokeTests` also publishes them with Native AOT. Add a check to `Checks.cs` for a new expectation.
- Never run `./build.ps1` without a target: the defaults are `ApiChecks`, `Benchmarks` and `CodeAnalysis` (needs a Sonar token).
- Code samples in `README.md` and `Docs/pages/` are not compiled; keep them in line with the tests by hand. New expectations belong in the README overview and in `Docs/pages/`.
- CONTRIBUTING requires SonarCloud issues to be solved and coverage above 90%.

## Documentation

Pages under `Docs/pages` are published on docs.testably.org and follow the [docs style guide](https://github.com/Testably/Testably.Site/blob/main/Docs/style-guide.md). The key rules:

- US English, sentence case for headings and labels, no em dashes, one line per paragraph.
- A `description` in the front matter of every page; the H1 is the title; order with unique `NN-` prefixes.
- Relative file links within the repository, site-absolute `/Library/...` links across libraries.
- Install instructions only through `<PackageInstall package="..." />`, never hand-written commands or NuGet badges.
- Examples check results with `await That(...)`; complete examples use TUnit `[Test]`.

`README.md` is the GitHub and NuGet landing page and is maintained separately from `Docs/pages/00-index.md`; keep their overview tables in sync by hand.

## Git workflow

- Branch from `main` and open a PR against `main`; PRs are squash-merged.
- The commit message is the PR description: the subject is the PR title with a conventional prefix (`feat:`, `fix:`, `chore:`, `ci:`, `docs:`, `!` for breaking changes), the body explains why (recent commits add a short code sample of the new API). Each paragraph is a single line. No co-author or attribution trailers.
- Never commit `.claude/`, `Artifacts/` or `TestResults/`.
- Dependabot PRs are auto-merged by Mergify once the unit and API tests pass. A `v*` tag publishes to nuget.org.

## References

Read these only when the task needs them:

- `Docs/agents/design-goals.md`: the goals and code rules for every change, the checks before a commit and the goals of this repository.
- `Docs/agents/workflows.md`: build, test, API, AOT, benchmark, mutation and documentation commands; pipeline targets; CI workflows.
- `Docs/agents/project-structure.md`: source layout, expectation architecture and where to add things.
- `CONTRIBUTING.md`: contribution and quality gate rules.
