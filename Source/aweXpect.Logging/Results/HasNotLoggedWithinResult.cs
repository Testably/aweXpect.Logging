using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has not logged a matching record within a
///     timeout, which in addition to the options of a <see cref="HasNotLoggedResult" /> allows specifying how often the
///     records are read.
/// </summary>
public sealed class HasNotLoggedWithinResult : HasNotLoggedResult
{
	internal HasNotLoggedWithinResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
	}

	/// <inheritdoc cref="HasLoggedWithinResult.CheckEvery(TimeSpan)" />
	public HasNotLoggedResult CheckEvery(TimeSpan interval)
	{
		RepeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
