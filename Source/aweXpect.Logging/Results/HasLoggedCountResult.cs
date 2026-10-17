using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has logged a matching record a number of
///     times.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
public class HasLoggedCountResult
	: AndOrResult<FakeLogCollector?, IThat<FakeLogCollector?>, HasLoggedCountResult>
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;
	private readonly IThat<FakeLogCollector?> _returnValue;

	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal HasLoggedCountResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue)
	{
		_returnValue = returnValue;
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="HasLoggedResult.Within(TimeSpan)" />
	public HasLoggedCountWithinResult Within(TimeSpan timeout)
	{
		_repeatedCheckOptions.Within(timeout);
		return new HasLoggedCountWithinResult(ExpectationBuilder, _returnValue, _repeatedCheckOptions);
	}
}
