using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a matching record a number of
///     times.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
public class LoggedCountResult
	: AndOrResult<FakeLogCollector?, IThat<FakeLogCollector?>, LoggedCountResult>
{
	private readonly RepeatedCheckOptions _repeatedCheckOptions;
	private readonly IThat<FakeLogCollector?> _returnValue;

	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal LoggedCountResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue)
	{
		_returnValue = returnValue;
		_repeatedCheckOptions = repeatedCheckOptions;
	}

	/// <inheritdoc cref="LoggedResult.Within(TimeSpan)" />
	public LoggedCountWithinResult Within(TimeSpan timeout)
	{
		_repeatedCheckOptions.Within(timeout);
		return new LoggedCountWithinResult(ExpectationBuilder, _returnValue, _repeatedCheckOptions);
	}
}
