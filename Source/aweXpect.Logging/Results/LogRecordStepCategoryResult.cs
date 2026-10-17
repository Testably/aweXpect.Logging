using aweXpect.Core;
using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step with a matching category, which in addition to the options of a <see cref="LogRecordStepResult" />
///     allows specifying how the category is compared, e.g. via
///     <see cref="StringEqualityOptionsExtensions.AsPrefix{TResult}(TResult)" />.
/// </summary>
public sealed class LogRecordStepCategoryResult
	: LogRecordStepResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly StringEqualityOptions _options;

	internal LogRecordStepCategoryResult(LogRecordFilter filter, StringEqualityOptions options)
		: base(filter)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	StringEqualityOptions IOptionsProvider<StringEqualityOptions>.Options => _options;
}
