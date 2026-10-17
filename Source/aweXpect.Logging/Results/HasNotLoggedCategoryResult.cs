using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has not logged a record with a matching
///     category, which in addition to the options of a <see cref="HasNotLoggedResult" /> allows specifying how the
///     category is compared, e.g. via <see cref="StringEqualityOptionsExtensions.AsPrefix{TResult}(TResult)" />.
/// </summary>
public sealed class HasNotLoggedCategoryResult
	: HasNotLoggedResult,
		IOptionsProvider<StringEqualityOptions>,
		IStringMatchTypeOptions
{
	private readonly StringEqualityOptions _options;

	internal HasNotLoggedCategoryResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions, StringEqualityOptions options)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
		_options = options;
	}

	/// <inheritdoc cref="IOptionsProvider{TOptions}.Options" />
	StringEqualityOptions IOptionsProvider<StringEqualityOptions>.Options => _options;
}
