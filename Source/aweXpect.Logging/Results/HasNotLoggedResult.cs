using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has not logged a matching record.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
/// <remarks>
///     It offers no quantifiers, as any amount but zero contradicts the expectation; a limited amount of records is
///     verified with <see cref="HasLoggedResult.AtMost(Times)" /> instead.
/// </remarks>
public class HasNotLoggedResult : LogRecordFilterResult<HasNotLoggedResult>
{
	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal HasNotLoggedResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
	}

	/// <inheritdoc cref="HasLoggedResult.Within(TimeSpan)" />
	public HasNotLoggedWithinResult Within(TimeSpan timeout)
	{
		RepeatedCheckOptions.Within(timeout);
		return new HasNotLoggedWithinResult(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions);
	}

	/// <inheritdoc cref="HasLoggedResult.WithCategory(string)" />
	public HasNotLoggedCategoryResult WithCategory(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddCategory(expected));

	/// <summary>
	///     Only considers records whose message is equal to the <paramref name="expected" /> message.
	/// </summary>
	/// <remarks>
	///     The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c> or
	///     <c>IgnoringCase()</c>.
	/// </remarks>
	public HasNotLoggedMessageResult WithMessage(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddMessage(expected));

	/// <summary>
	///     Only considers records with a property <paramref name="name" />, e.g. <c>Attempt</c> for the message
	///     template <c>Retry {Attempt}</c>.
	/// </summary>
	/// <remarks>
	///     Its value can be verified on the result with <see cref="HasNotLoggedPropertyResult.WithValue(string)" />.
	/// </remarks>
	public HasNotLoggedPropertyResult WithProperty(string name)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddProperty(name));
}
