using System;
using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged a matching record.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
/// <remarks>
///     The quantifiers are declared here instead of implementing <see cref="IOptionsProvider{TOptions}" /> for the
///     <see cref="Quantifier" />, because the <see cref="QuantifierExtensions" /> would return this type again, while a
///     quantified result must be a <see cref="HasLoggedCountResult" /> that only offers the options still applying to it.
/// </remarks>
public class HasLoggedResult : LogRecordFilterResult<HasLoggedResult>
{
	private readonly Quantifier _quantifier;
	private readonly List<LogRecordFilter> _steps;

	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal HasLoggedResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
		_quantifier = quantifier;
		_steps = steps;
	}

	/// <summary>
	///     Verifies that it logged matching records at least…
	/// </summary>
	public CountTimesResult<HasLoggedCountResult> AtLeast()
		=> new(minimum => AtLeast(minimum));

	/// <summary>
	///     Verifies that it logged matching records at least <paramref name="minimum" /> times.
	/// </summary>
	public HasLoggedCountResult AtLeast(Times minimum)
	{
		_quantifier.AtLeast(minimum.Value);
		return Quantified();
	}

	/// <summary>
	///     Verifies that it logged matching records at most…
	/// </summary>
	public CountTimesResult<HasLoggedCountResult> AtMost()
		=> new(maximum => AtMost(maximum));

	/// <summary>
	///     Verifies that it logged matching records at most <paramref name="maximum" /> times.
	/// </summary>
	public HasLoggedCountResult AtMost(Times maximum)
	{
		_quantifier.AtMost(maximum.Value);
		return Quantified();
	}

	/// <summary>
	///     Verifies that it logged matching records between <paramref name="minimum" />…
	/// </summary>
	public BetweenResult<HasLoggedCountResult> Between(int minimum)
		=> new(maximum =>
		{
			_quantifier.Between(minimum, maximum);
			return Quantified();
		});

	/// <summary>
	///     Verifies that it logged matching records exactly <paramref name="expected" /> times.
	/// </summary>
	public HasLoggedCountResult Exactly(Times expected)
	{
		_quantifier.Exactly(expected.Value);
		return Quantified();
	}

	/// <summary>
	///     Verifies that it logged matching records fewer than…
	/// </summary>
	public CountTimesResult<HasLoggedCountResult> LessThan()
		=> new(maximum => LessThan(maximum));

	/// <summary>
	///     Verifies that it logged matching records fewer than <paramref name="maximum" /> times.
	/// </summary>
	public HasLoggedCountResult LessThan(Times maximum)
	{
		_quantifier.LessThan(maximum.Value);
		return Quantified();
	}

	/// <summary>
	///     Verifies that it logged matching records more than…
	/// </summary>
	public CountTimesResult<HasLoggedCountResult> MoreThan()
		=> new(minimum => MoreThan(minimum));

	/// <summary>
	///     Verifies that it logged matching records more than <paramref name="minimum" /> times.
	/// </summary>
	public HasLoggedCountResult MoreThan(Times minimum)
	{
		_quantifier.MoreThan(minimum.Value);
		return Quantified();
	}

	/// <summary>
	///     Verifies that it never logged a matching record.
	/// </summary>
	public HasLoggedCountResult Never()
		=> Exactly(0);

	/// <summary>
	///     Verifies that it logged exactly one matching record.
	/// </summary>
	public HasLoggedCountResult Once()
		=> Exactly(1);

	/// <summary>
	///     Verifies that it logged exactly two matching records.
	/// </summary>
	public HasLoggedCountResult Twice()
		=> Exactly(2);

	/// <summary>
	///     Verifies that the records of the <paramref name="steps" /> were logged after a matching record, in the given
	///     order.
	/// </summary>
	/// <remarks>
	///     Each step must match a record that was logged after the one of the previous step, so a record cannot match
	///     two steps, while other records may be logged in between. A timeout for all records can be specified on the
	///     result.
	/// </remarks>
	/// <exception cref="ArgumentNullException">The <paramref name="steps" /> are <see langword="null" />.</exception>
	public HasLoggedInOrderResult Then(params Func<LogRecordStep, LogRecordStepResult>[] steps)
	{
		if (steps is null)
		{
			throw new ArgumentNullException(nameof(steps));
		}

		LogRecordStep step = new();
		foreach (Func<LogRecordStep, LogRecordStepResult> selectStep in steps)
		{
			_steps.Add(selectStep(step).Filter);
		}

		return new HasLoggedInOrderResult(ExpectationBuilder, ReturnValue, RepeatedCheckOptions);
	}

	/// <summary>
	///     Allows a <paramref name="timeout" /> until the expectation must be met, e.g. for the records of a background
	///     service.
	/// </summary>
	/// <remarks>
	///     The records are read repeatedly until their amount decides the outcome: an expectation that further records
	///     can meet, e.g. <c>HasLogged</c>, succeeds as soon as it is met, while one that further records can violate,
	///     e.g. <c>HasNotLogged</c> or <c>AtMost</c>, waits for the full timeout and fails as soon as it is violated.
	///     How often the records are read can be specified on the result.
	/// </remarks>
	public HasLoggedWithinResult Within(TimeSpan timeout)
	{
		RepeatedCheckOptions.Within(timeout);
		return new HasLoggedWithinResult(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions,
			_steps);
	}

	/// <summary>
	///     Only counts records whose category is equal to the <paramref name="expected" /> category.
	/// </summary>
	/// <remarks>
	///     The category of an <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}" /> is the full name of
	///     its type argument, e.g. <c>MyApp.Services.OrderService</c>. The match type and the comparison can be
	///     changed on the result, e.g. with <c>AsPrefix()</c> or <c>AsWildcard()</c>.
	/// </remarks>
	public HasLoggedCategoryResult WithCategory(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions, _steps,
			Filter.AddCategory(expected));

	/// <summary>
	///     Only counts records whose message is equal to the <paramref name="expected" /> message.
	/// </summary>
	/// <remarks>
	///     The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c> or
	///     <c>IgnoringCase()</c>.
	/// </remarks>
	public HasLoggedMessageResult WithMessage(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions, _steps,
			Filter.AddMessage(expected));

	/// <summary>
	///     Only counts records with a property <paramref name="name" />, e.g. <c>Attempt</c> for the message template
	///     <c>Retry {Attempt}</c>.
	/// </summary>
	/// <remarks>
	///     Its value can be verified on the result with <see cref="HasLoggedPropertyResult.WithValue(string)" />.
	/// </remarks>
	public HasLoggedPropertyResult WithProperty(string name)
		=> new(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions, _steps,
			Filter.AddProperty(name));

	private HasLoggedCountResult Quantified()
		=> new(ExpectationBuilder, ReturnValue, RepeatedCheckOptions);
}
