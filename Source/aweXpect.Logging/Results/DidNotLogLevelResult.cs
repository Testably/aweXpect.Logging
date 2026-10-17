using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> did not log a record with a log level,
///     which in addition to the options of a <see cref="DidNotLogResult" /> allows extending the level to more severe
///     ones.
/// </summary>
public sealed class DidNotLogLevelResult : DidNotLogResult
{
	internal DidNotLogLevelResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
	}

	/// <summary>
	///     Also considers records with a more severe level, e.g. an <c>Error</c> for an unexpected <c>Warning</c>.
	/// </summary>
	public DidNotLogResult OrHigher()
	{
		Filter.OrHigher();
		return this;
	}
}
