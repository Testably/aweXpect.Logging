using aweXpect.Core;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged a matching record a number of
///     times.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
public sealed class HasLoggedCountResult
	: AndOrResult<FakeLogCollector?, IThat<FakeLogCollector?>, HasLoggedCountResult>
{
	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal HasLoggedCountResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue)
		: base(expectationBuilder, returnValue)
	{
	}
}
