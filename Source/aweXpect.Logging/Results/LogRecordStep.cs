using aweXpect.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     Specifies a record that a <see cref="FakeLogCollector" /> logged after the record of the previous step, in
///     <see cref="HasLoggedResult.Then(System.Func{LogRecordStep, LogRecordStepResult}[])" />.
/// </summary>
public sealed class LogRecordStep
{
	internal LogRecordStep()
	{
	}

	/// <summary>
	///     Expects a record of any level.
	/// </summary>
	public LogRecordStepResult HasLogged()
		=> new(new LogRecordFilter(null));

	/// <summary>
	///     Expects a record with the <paramref name="level" />.
	/// </summary>
	public LogRecordStepLevelResult HasLogged(LogLevel level)
		=> new(new LogRecordFilter(level));
}
