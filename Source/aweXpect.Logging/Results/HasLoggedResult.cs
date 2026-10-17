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

	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal HasLoggedResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
		_quantifier = quantifier;
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
	///     Only counts records whose message is equal to the <paramref name="expected" /> message.
	/// </summary>
	/// <remarks>
	///     The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c> or
	///     <c>IgnoringCase()</c>.
	/// </remarks>
	public HasLoggedMessageResult WithMessage(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions, Filter.AddMessage(expected));

	private HasLoggedCountResult Quantified()
		=> new(ExpectationBuilder, ReturnValue, RepeatedCheckOptions);
}
