using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged a record with a log level, which
///     in addition to the options of a <see cref="HasLoggedResult" /> allows extending the level to more severe ones.
/// </summary>
public sealed class HasLoggedLevelResult : HasLoggedResult
{
	private readonly LogRecordFilter _filter;

	internal HasLoggedLevelResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier)
		: base(expectationBuilder, returnValue, filter, quantifier)
	{
		_filter = filter;
	}

	/// <summary>
	///     Also counts records with a more severe level, e.g. an <c>Error</c> for an expected <c>Warning</c>.
	/// </summary>
	public HasLoggedResult OrHigher()
	{
		_filter.OrHigher();
		return this;
	}
}
