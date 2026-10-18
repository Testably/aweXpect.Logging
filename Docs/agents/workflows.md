# Workflows

All commands run from the repository root. `build.ps1` (Windows), `build.sh` (Linux, macOS) and `build.cmd` (either) build `Pipeline/Build.csproj` and run the given Fallout targets; they install the SDK from `global.json` only when no `dotnet` is on the path.

## Build and test

```powershell
dotnet build aweXpect.Logging.slnx
dotnet test Tests/aweXpect.Logging.Tests                                   # net8.0, net10.0, net48 (Windows)
dotnet test Tests/aweXpect.Logging.Tests --no-build -f net10.0 --filter "FullyQualifiedName~WithCategoryTests"
dotnet test Tests/aweXpect.Logging.Tests --no-build -f net48               # Windows only
```

A failing expectation throws `XunitException` in the unit tests, so tests for failures assert `Throws<XunitException>().WithMessage("""...""")` with the complete message. `LogLevels.Data` provides the log levels for theories.

## Public API

```powershell
dotnet test Tests/aweXpect.Logging.Api.Tests                                       # compare
dotnet test Tests/aweXpect.Logging.Api.Tests --filter "Name=AcceptApiChanges"      # accept
```

`ApiApprovalTests` generates the public API of `Source/aweXpect.Logging/bin/<Configuration>/<tfm>/aweXpect.Logging.dll` with PublicApiGenerator and compares it with `Expected/aweXpect.Logging_<tfm>.txt` for every target framework in `Source/Directory.Build.props`. The configuration is the one the API test project was built with, so build both in the same configuration. `AcceptApiChanges` is `[Explicit]` and only runs when selected by the filter; it overwrites the expected files, which become part of the PR. There is no `PublicAPI.Shipped.txt` / `Unshipped.txt` in this repository.

## AOT smoke tests

```powershell
dotnet run --project Tests/Aot/aweXpect.Logging.Aot     # checks under the JIT
./build.ps1 AotSmokeTests                               # JIT run, then Native AOT publish and run
```

The program prints `PASS` / `FAIL` per check from `Checks.cs` and returns a non-zero exit code on any failure. `AotSmokeTests` publishes for the current runtime identifier into `Artifacts/Aot/`, which needs the platform's native toolchain (on Windows, the Visual Studio C++ build tools). Trimmer and ILC warnings fail the publish (`ILLinkTreatWarningsAsErrors`, `IlcTreatWarningsAsErrors`).

## Pipeline targets

List them with `./build.ps1 --help`. Always name a target: without one, the defaults `ApiChecks`, `Benchmarks` and `CodeAnalysis` run. The configuration is `Debug` locally and `Release` on a server unless `--configuration` is passed. Outputs go to `Artifacts/` and `TestResults/` (both git-ignored and cleaned by `Clean`).

| Target | Purpose |
|--------|---------|
| `Compile` | Restore (`Clean`, `Restore`) and build the solution with the GitVersion version (`-DEV` locally, `-CI` on CI, tag suffix on releases). |
| `UnitTests` | `dotnet test` of `aweXpect.Logging.Tests` per non-net48 framework with coverage, plus net48 through the xUnit console runner on Windows. |
| `CodeCoverage` | `UnitTests`, then an OpenCover report via ReportGenerator. |
| `ApiChecks` | Runs `aweXpect.Logging.Api.Tests`. |
| `AotSmokeTests` | See above. |
| `MutationTests` | Stryker on `aweXpect.Logging` with `aweXpect.Logging.Tests` (net10.0, mutation level Advanced). Off `main` it only mutates code changed since `main`. Writes reports to `Artifacts/Stryker/`. Slow; leave it to CI unless asked. |
| `MutationTestDashboard` | CI only: uploads the report to the Stryker dashboard and comments on the PR. |
| `Benchmarks` | Builds the benchmarks in Release and runs all of them (`BenchmarkDotNet`, `BenchmarkResult`); results in `Artifacts/Benchmarks/`. |
| `BenchmarkComment` | CI only: posts the benchmark table on the PR; rows whose method name contains `_aweXpect` are bold. |
| `CodeAnalysis` | SonarCloud analysis around `Compile` and `CodeCoverage`; needs `SonarToken`. |
| `Pack` | `UpdateReadme` (rewrites the README badges for the release into `Artifacts/README.md`, which becomes the package readme) and `Compile`, then moves the `.nupkg` / `.snupkg` files to `Artifacts/Packages/`. |

## Benchmarks

```powershell
./build.ps1 Benchmarks
dotnet run -c Release --project Benchmarks/aweXpect.Logging.Benchmarks -- --filter "*"
```

Benchmarks are `[Benchmark]` methods of the partial class `HappyCaseBenchmarks` in files like `HappyCaseBenchmarks.Dummy.cs`, named `<Case>_aweXpect` so that the PR comment highlights them. Currently only the placeholder `Dummy_aweXpect` exists. `-- --list flat` lists the benchmarks without running them.

## Documentation

- `Docs/pages/*.md` is published to docs.testably.org: after a successful `Build` workflow on `main`, `notify-docs-site.yml` dispatches a rebuild of `Testably/Testably.Site`. `00-index.md` embeds `README.md`.
- Code fences are not compiled. Copy samples from passing tests, and copy failure messages from the `WithMessage` assertions.
- New expectations go into the overview table of `README.md` and the matching page in `Docs/pages/`.

## CI workflows

| Workflow | Trigger | Jobs |
|----------|---------|------|
| `ci.yml` | PR to `main` | Unit tests (`CodeCoverage`) and AOT smoke tests on Ubuntu, Windows and macOS; API tests; mutation tests; benchmarks (not for Dependabot); SonarCloud (not for Dependabot or forks); test result publishing. |
| `ci-analysis.yml` | After `CI` completes | `MutationTestDashboard` and `BenchmarkComment` with the artifacts of the CI run. |
| `build.yml` | Push to `main`, `v*` tags | The same checks plus `Pack`. On a tag, `Push` waits for pack, unit, AOT, API, test result and SonarCloud jobs, pushes to nuget.org and creates the GitHub release; `Finalize release` comments on the released issues and PRs. |
| `notify-docs-site.yml` | After `Build` on `main` | Triggers the docs site rebuild. |

`build.yml` pins actions to commit SHAs. Dependabot updates GitHub Actions and NuGet weekly with the `chore` prefix (groups for `Fallout*`, `xunit*` and `aweXpect*`), and Mergify squash-merges its PRs once the three unit test jobs and the API tests pass.
