using aweXpect.Core;
using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step with a matching message template, which in addition to the options of a
///     <see cref="LogRecordStepResult" /> allows specifying how the template is compared, e.g. via
///     <see cref="StringEqualityOptionsExtensions.AsWildcard{TResult}(TResult)" />.
/// </summary>
public sealed class LogRecordStepMessageTemplateResult
	: LogRecordStepResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly OptionsHolder<StringEqualityOptions> _options;

	internal LogRecordStepMessageTemplateResult(LogRecordFilter filter, OptionsHolder<StringEqualityOptions> options)
		: base(filter)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	OptionsHolder<StringEqualityOptions> IOptionsProvider<StringEqualityOptions>.Options => _options;
}
