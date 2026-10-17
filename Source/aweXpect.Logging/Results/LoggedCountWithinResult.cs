using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a matching record a number of
///     times within a timeout, which in addition to the options of a <see cref="LoggedCountResult" /> allows
///     specifying how often the records are read.
/// </summary>
public sealed class LoggedCountWithinResult : LoggedCountResult
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;

	internal LoggedCountWithinResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, repeatedCheckOptions)
	{
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="LoggedWithinResult.CheckEvery(TimeSpan)" />
	public LoggedCountResult CheckEvery(TimeSpan interval)
	{
		_repeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
