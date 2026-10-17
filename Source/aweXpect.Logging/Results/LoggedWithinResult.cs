using System;
using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a matching record within a
///     timeout, which in addition to the options of a <see cref="LoggedResult" /> allows specifying how often the
///     records are read.
/// </summary>
public sealed class LoggedWithinResult : LoggedResult
{
	internal LoggedWithinResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps)
		: base(expectationBuilder, returnValue, filter, quantifier, repeatedCheckOptions, steps)
	{
	}

	/// <summary>
	///     Sets the <paramref name="interval" /> in which the records are read until the timeout.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>Customize.aweXpect.Settings().DefaultCheckInterval</c>, i.e. 100 milliseconds, if not
	///     specified.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="interval" /> is not positive.</exception>
	public LoggedResult CheckEvery(TimeSpan interval)
	{
		RepeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
