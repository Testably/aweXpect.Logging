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
		LogRecordFilter filter)
		: base(expectationBuilder, returnValue, filter)
	{
	}

	/// <summary>
	///     Only considers records whose message is equal to the <paramref name="expected" /> message.
	/// </summary>
	/// <remarks>
	///     The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c> or
	///     <c>IgnoringCase()</c>.
	/// </remarks>
	public HasNotLoggedMessageResult WithMessage(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, Filter.AddMessage(expected));
}
