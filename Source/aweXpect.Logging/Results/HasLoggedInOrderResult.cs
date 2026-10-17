using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged matching records in order.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
public sealed class HasLoggedInOrderResult
	: AndOrResult<FakeLogCollector?, IThat<FakeLogCollector?>, HasLoggedInOrderResult>
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;

	internal HasLoggedInOrderResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue)
	{
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="LogRecordFilterResult{TSelf}.CheckEvery(TimeSpan)" />
	public HasLoggedInOrderResult CheckEvery(TimeSpan interval)
	{
		_repeatedCheckOptions.CheckEveryAfterWithin(interval);
		return this;
	}

	/// <summary>
	///     Allows a <paramref name="timeout" /> until all records must have been logged in order, e.g. for the records
	///     of a background service.
	/// </summary>
	/// <remarks>
	///     The records are read repeatedly until they were logged in order, as further records cannot undo that.
	/// </remarks>
	public HasLoggedInOrderResult Within(TimeSpan timeout)
	{
		_repeatedCheckOptions.Within(timeout);
		return this;
	}
}
