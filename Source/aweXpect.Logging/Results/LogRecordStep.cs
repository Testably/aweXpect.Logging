using System.Diagnostics.CodeAnalysis;
using aweXpect.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     Specifies a record that a <see cref="FakeLogCollector" /> logged after the record of the previous step, in
///     <see cref="LoggedResult.Then(System.Func{LogRecordStep, LogRecordStepResult}[])" />.
/// </summary>
public sealed class LogRecordStep
{
	internal LogRecordStep()
	{
	}

	/// <summary>
	///     Expects a record of any level.
	/// </summary>
	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "It is called on the parameter of the step, e.g. r => r.Logged().")]
	public LogRecordStepResult Logged()
		=> new(new LogRecordFilter(null));

	/// <summary>
	///     Expects a record with the <paramref name="level" />.
	/// </summary>
	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "It is called on the parameter of the step, e.g. r => r.Logged(LogLevel.Warning).")]
	public LogRecordStepLevelResult Logged(LogLevel level)
		=> new(new LogRecordFilter(level));

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Trace" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedTrace()
		=> Logged(LogLevel.Trace);

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Debug" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedDebug()
		=> Logged(LogLevel.Debug);

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Information" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedInformation()
		=> Logged(LogLevel.Information);

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Warning" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedWarning()
		=> Logged(LogLevel.Warning);

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Error" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedError()
		=> Logged(LogLevel.Error);

	/// <summary>
	///     Expects a record with the level <see cref="LogLevel.Critical" />.
	/// </summary>
	public LogRecordStepLevelResult LoggedCritical()
		=> Logged(LogLevel.Critical);
}
