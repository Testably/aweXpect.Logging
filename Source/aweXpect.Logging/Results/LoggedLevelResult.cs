using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a record with a log level, which
///     in addition to the options of a <see cref="LoggedResult" /> allows extending the level to more severe ones.
/// </summary>
public sealed class LoggedLevelResult : LoggedResult
{
	internal LoggedLevelResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps)
		: base(expectationBuilder, returnValue, filter, quantifier, repeatedCheckOptions, steps)
	{
	}

	/// <summary>
	///     Also counts records with a more severe level, e.g. an <c>Error</c> for an expected <c>Warning</c>.
	/// </summary>
	public LoggedResult OrHigher()
	{
		Filter.OrHigher();
		return this;
	}
}
