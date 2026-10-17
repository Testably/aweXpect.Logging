using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has not logged a record with a log level,
///     which in addition to the options of a <see cref="HasNotLoggedResult" /> allows extending the level to more severe
///     ones.
/// </summary>
public sealed class HasNotLoggedLevelResult : HasNotLoggedResult
{
	internal HasNotLoggedLevelResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter)
		: base(expectationBuilder, returnValue, filter)
	{
	}

	/// <summary>
	///     Also considers records with a more severe level, e.g. an <c>Error</c> for an unexpected <c>Warning</c>.
	/// </summary>
	public HasNotLoggedResult OrHigher()
	{
		Filter.OrHigher();
		return this;
	}
}
