using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a record with a matching property
///     value, which in addition to the options of a <see cref="LoggedResult" /> allows specifying how the value is
///     compared, e.g. via <see cref="StringEqualityOptionsExtensions.AsWildcard{TResult}(TResult)" />.
/// </summary>
public sealed class LoggedPropertyValueResult
	: LoggedResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly OptionsHolder<StringEqualityOptions> _options;

	internal LoggedPropertyValueResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, OptionsHolder<Quantifier> quantifier, OptionsHolder<RepeatedCheckOptions> repeatedCheckOptions,
		List<LogRecordFilter> steps, OptionsHolder<StringEqualityOptions> options)
		: base(expectationBuilder, returnValue, filter, quantifier, repeatedCheckOptions, steps)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	OptionsHolder<StringEqualityOptions> IOptionsProvider<StringEqualityOptions>.Options => _options;
}
