# Workflows

All commands run from the repository root. `build.ps1` (Windows), `build.sh` (Linux, macOS) and `build.cmd` (either) build `Pipeline/Build.csproj` and run the given Fallout targets; they install the SDK from `global.json` only when no `dotnet` is on the path.

## Build and test

```powershell
dotnet build aweXpect.Logging.slnx
dotnet test                                                              # all test projects and frameworks
dotnet test --project Tests/aweXpect.Logging.Tests                       # net8.0, net10.0, net48 (Windows only)
dotnet test --project Tests/aweXpect.Logging.Tests --no-build -f net10.0 --treenode-filter "/*/*/*WithCategoryTests/*"
```

All test projects use TUnit and are executables of the Microsoft.Testing.Platform, which `global.json` opts into for `dotnet test`. `--treenode-filter "/<assembly>/<namespace>/<class>/<test>"` selects tests with `*` as wildcard. `dotnet run --project Tests/aweXpect.Logging.Tests -f net10.0` starts a single target framework directly; arguments after `--` go to the test application, for example `--help`.

A failing expectation throws `aweXpect.FailException` in the unit tests, so tests for failures assert `Throws<FailException>().WithMessage("""...""")` with the complete message. `LogLevels.Data` provides the log levels for `[MethodDataSource]` tests.

## Public API

```powershell
dotnet test --project Tests/aweXpect.Logging.Api.Tests                                                                      # compare
dotnet test --project Tests/aweXpect.Logging.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"     # accept
```

`ApiApprovalTests` generates the public API of `Source/aweXpect.Logging/bin/<Configuration>/<tfm>/aweXpect.Logging.dll` with `aweXpect.Snapshots.PublicApi` and compares it with the snapshot `ApiApprovalTests.aweXpect%2ELogging.<tfm>.awexpect-snapshot.txt` (dots encoded as `%2E`) next to the tests for every target framework in `Source/Directory.Build.props`. The configuration is the one the API test project was built with, so build both in the same configuration. A mismatch writes a `*.awexpect-snapshot.pending.*` file, which is never committed. `AcceptApiChanges` is `[Explicit]`: TUnit only runs it when every test the filter matches is explicit, so it never runs with the other tests or in CI; it overwrites the snapshots, which become part of the PR. There is no `PublicAPI.Shipped.txt` / `Unshipped.txt` in this repository.

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
| `UnitTests` | Runs `aweXpect.Logging.Tests` with `dotnet run` for every target framework (net48 only on Windows) and writes `TestResults/<project>_<framework>.trx` and `.cobertura.xml`. |
| `CodeCoverage` | `UnitTests`, then merges the coverage into `TestResults/reports/SonarQube.xml` via ReportGenerator. |
| `ApiChecks` | Runs `aweXpect.Logging.Api.Tests`. |
| `AotSmokeTests` | See above. |
| `MutationTests` | Stryker (the version of the `PackageDownload` in `Pipeline/Build.csproj`) on `aweXpect.Logging` with `aweXpect.Logging.Tests` and `aweXpect.Logging.Api.Tests` (net10.0, mutation level Advanced, `--test-runner mtp`). Off `main` it only mutates code changed since `main`. Writes reports to `Artifacts/Stryker/`. Slow; leave it to CI unless asked. Stryker writes the active mutant to fixed file names in `%TEMP%`, so concurrent local runs need separate `TEMP`/`TMP` folders. |
| `MutationTestsDashboard` | CI only: uploads the report to the Stryker dashboard. |
| `MutationTestsComment` | CI only: `MutationTestsDashboard`, then comments the result on the PR. |
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
| `ci-analysis.yml` | After `CI` completes | `MutationTestsComment` and `BenchmarkComment` with the artifacts of the CI run. |
| `build.yml` | Push to `main`, `v*` tags | The same checks plus `Pack`. On a tag, `Push` waits for pack, unit, AOT, API, test result and SonarCloud jobs, pushes to nuget.org and creates the GitHub release; `Finalize release` comments on the released issues and PRs. |
| `notify-docs-site.yml` | After `Build` on `main` | Triggers the docs site rebuild. |

`build.yml` pins actions to commit SHAs. Dependabot updates GitHub Actions and NuGet weekly with the `chore` prefix (groups for `Fallout*`, `TUnit*` and `aweXpect*`), and Mergify squash-merges its PRs once the three unit test jobs and the API tests pass.
