using System;
using System.Linq;
using Fallout.Common;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	const string NetFramework = "net48";

	Target UnitTests => _ => _
		.DependsOn(DotNetFrameworkUnitTests)
		.DependsOn(DotNetUnitTests);

	Project[] UnitTestProjects =>
	[
		Solution.Tests.aweXpect_Logging_Tests,
	];

	Target DotNetFrameworkUnitTests => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.OnlyWhenDynamic(() => EnvironmentInfo.IsWin)
		.Executes(() => RunUnitTests(framework => framework == NetFramework));

	Target DotNetUnitTests => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.Executes(() => RunUnitTests(framework => framework != NetFramework));

	/// <summary>
	///     Runs the unit test projects for their target frameworks that match <paramref name="includeFramework" />, and
	///     writes a TRX file and a Cobertura coverage file per project and target framework into the test results.
	/// </summary>
	/// <remarks>
	///     The test projects are executables of the Microsoft.Testing.Platform. They are started with <c>dotnet run</c>
	///     per target framework, so that each run writes its results to files of its own.
	/// </remarks>
	void RunUnitTests(Func<string, bool> includeFramework)
	{
		(Project Project, string Framework)[] testRuns = UnitTestProjects
			.SelectMany(project => (project.GetTargetFrameworks() ?? [])
				.Where(includeFramework)
				.Select(framework => (project, framework)))
			.ToArray();

		Assert.NotEmpty(testRuns);

		DotNetRun(s => s
				.SetConfiguration(Configuration)
				.SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
				.EnableNoBuild()
				.CombineWith(
					testRuns,
					(settings, testRun) => settings
						.SetProjectFile(testRun.Project)
						.SetFramework(testRun.Framework)
						.SetProcessAdditionalArguments(
							"--",
							"--disable-logo",
							$"--results-directory \"{TestResultsDirectory}\"",
							"--report-trx",
							$"--report-trx-filename {testRun.Project.Name}_{testRun.Framework}.trx",
							"--coverage",
							"--coverage-output-format cobertura",
							$"--coverage-output {testRun.Project.Name}_{testRun.Framework}.cobertura.xml")),
			completeOnFailure: true);
	}
}
