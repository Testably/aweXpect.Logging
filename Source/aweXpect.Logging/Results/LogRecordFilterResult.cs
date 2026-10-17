using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation on the records of a <see cref="FakeLogCollector" />, which allows narrowing down
///     the records that are considered.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
/// <remarks>
///     Shared by <see cref="HasLoggedResult" /> and <see cref="HasNotLoggedResult" />, so that a filter returns the
///     type it was called on and keeps its options available.
/// </remarks>
public abstract class LogRecordFilterResult<TSelf>
	: AndOrResult<FakeLogCollector?, IThat<FakeLogCollector?>, TSelf>
	where TSelf : LogRecordFilterResult<TSelf>
{
	private protected LogRecordFilterResult(ExpectationBuilder expectationBuilder,
		IThat<FakeLogCollector?> returnValue, LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue)
	{
		ReturnValue = returnValue;
		Filter = filter;
		RepeatedCheckOptions = repeatedCheckOptions;
	}

	private protected LogRecordFilter Filter { get; }

	private protected RepeatedCheckOptions RepeatedCheckOptions { get; }

	private protected IThat<FakeLogCollector?> ReturnValue { get; }

	/// <summary>
	///     Allows a <paramref name="timeout" /> until the expectation must be met, e.g. for the records of a background
	///     service.
	/// </summary>
	/// <remarks>
	///     The records are read repeatedly until their amount decides the outcome: an expectation that further records
	///     can meet, e.g. <c>HasLogged</c>, succeeds as soon as it is met, while one that further records can violate,
	///     e.g. <c>HasNotLogged</c> or <c>AtMost</c>, waits for the full timeout and fails as soon as it is violated.
	/// </remarks>
	public TSelf Within(TimeSpan timeout)
	{
		RepeatedCheckOptions.Within(timeout);
		return (TSelf)this;
	}

	/// <summary>
	///     Only considers records with an exception of type <typeparamref name="TException" /> or a derived type.
	/// </summary>
	public TSelf WithException<TException>()
		where TException : Exception
	{
		Filter.AddException(typeof(TException));
		return (TSelf)this;
	}
}
