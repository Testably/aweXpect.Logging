using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged a record with a matching property
///     value, which in addition to the options of a <see cref="HasLoggedResult" /> allows specifying how the value is
///     compared, e.g. via <see cref="StringEqualityOptionsExtensions.AsWildcard{TResult}(TResult)" />.
/// </summary>
public sealed class HasLoggedPropertyValueResult
	: HasLoggedResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly StringEqualityOptions _options;

	internal HasLoggedPropertyValueResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps, StringEqualityOptions options)
		: base(expectationBuilder, returnValue, filter, quantifier, repeatedCheckOptions, steps)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	StringEqualityOptions IOptionsProvider<StringEqualityOptions>.Options => _options;
}
