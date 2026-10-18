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
///     Shared by <see cref="LoggedResult" /> and <see cref="DidNotLogResult" />, so that a filter returns the
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
	///     Only considers records that were logged by an
	///     <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}" /> for <typeparamref name="TCategory" />.
	/// </summary>
	/// <remarks>
	///     The category of the logger is the full name of the type, e.g. <c>MyApp.Services.OrderService</c>, and is
	///     compared exactly.
	/// </remarks>
	public TSelf WithCategory<TCategory>()
	{
		Filter.AddCategory(LogRecordFilter.GetCategoryName(typeof(TCategory)));
		return (TSelf)this;
	}

	/// <summary>
	///     Only considers records whose event ID is equal to the <paramref name="eventId" />.
	/// </summary>
	public TSelf WithEventId(int eventId)
	{
		Filter.AddEventId(eventId);
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
