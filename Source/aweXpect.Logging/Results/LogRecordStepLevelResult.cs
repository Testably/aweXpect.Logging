using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step with a log level, which in addition to the options of a <see cref="LogRecordStepResult" /> allows
///     extending the level to more severe ones.
/// </summary>
public sealed class LogRecordStepLevelResult : LogRecordStepResult
{
	internal LogRecordStepLevelResult(LogRecordFilter filter)
		: base(filter)
	{
	}

	/// <inheritdoc cref="LoggedLevelResult.OrHigher()" />
	public LogRecordStepResult OrHigher()
	{
		Filter.OrHigher();
		return this;
	}
}
