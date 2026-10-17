using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged matching records in order within
///     a timeout, which in addition to the options of a <see cref="LoggedInOrderResult" /> allows specifying how
///     often the records are read.
/// </summary>
public sealed class LoggedInOrderWithinResult : LoggedInOrderResult
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;

	internal LoggedInOrderWithinResult(ExpectationBuilder expectationBuilder,
		IThat<FakeLogCollector?> returnValue, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, repeatedCheckOptions)
	{
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="LoggedWithinResult.CheckEvery(TimeSpan)" />
	public LoggedInOrderResult CheckEvery(TimeSpan interval)
	{
		_repeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
