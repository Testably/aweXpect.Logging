using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step with a property, which in addition to the options of a <see cref="LogRecordStepResult" /> allows
///     verifying the value of the property.
/// </summary>
public sealed class LogRecordStepPropertyResult : LogRecordStepResult
{
	private readonly LogRecordFilter.PropertyCondition _property;

	internal LogRecordStepPropertyResult(LogRecordFilter filter, LogRecordFilter.PropertyCondition property)
		: base(filter)
	{
		_property = property;
	}

	/// <inheritdoc cref="LoggedPropertyResult.WithValue(string)" />
	public LogRecordStepPropertyValueResult WithValue(string? expected)
		=> new(Filter, _property.WithValue(expected));
}
