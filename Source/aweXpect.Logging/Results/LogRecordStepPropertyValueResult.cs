using aweXpect.Core;
using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step with a matching property value, which in addition to the options of a
///     <see cref="LogRecordStepResult" /> allows specifying how the value is compared, e.g. via
///     <see cref="StringEqualityOptionsExtensions.AsWildcard{TResult}(TResult)" />.
/// </summary>
public sealed class LogRecordStepPropertyValueResult
	: LogRecordStepResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly StringEqualityOptions _options;

	internal LogRecordStepPropertyValueResult(LogRecordFilter filter, StringEqualityOptions options)
		: base(filter)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	StringEqualityOptions IOptionsProvider<StringEqualityOptions>.Options => _options;
}
