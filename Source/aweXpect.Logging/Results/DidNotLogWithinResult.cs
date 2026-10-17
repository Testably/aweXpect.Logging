using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> did not log a matching record within a
///     timeout, which in addition to the options of a <see cref="DidNotLogResult" /> allows specifying how often the
///     records are read.
/// </summary>
public sealed class DidNotLogWithinResult : DidNotLogResult
{
	internal DidNotLogWithinResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
	}

	/// <inheritdoc cref="LoggedWithinResult.CheckEvery(TimeSpan)" />
	public DidNotLogResult CheckEvery(TimeSpan interval)
	{
		RepeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
