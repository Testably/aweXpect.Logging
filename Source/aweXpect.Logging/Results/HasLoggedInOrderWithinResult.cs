using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged matching records in order within
///     a timeout, which in addition to the options of a <see cref="HasLoggedInOrderResult" /> allows specifying how
///     often the records are read.
/// </summary>
public sealed class HasLoggedInOrderWithinResult : HasLoggedInOrderResult
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;

	internal HasLoggedInOrderWithinResult(ExpectationBuilder expectationBuilder,
		IThat<FakeLogCollector?> returnValue, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, repeatedCheckOptions)
	{
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="HasLoggedWithinResult.CheckEvery(TimeSpan)" />
	public HasLoggedInOrderResult CheckEvery(TimeSpan interval)
	{
		_repeatedCheckOptions.CheckEvery(interval);
		return this;
	}
}
