using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> logged a record with a property, which
///     in addition to the options of a <see cref="LoggedResult" /> allows verifying the value of the property.
/// </summary>
public sealed class LoggedPropertyResult : LoggedResult
{
	private readonly LogRecordFilter.PropertyCondition _property;
	private readonly Quantifier _quantifier;
	private readonly List<LogRecordFilter> _steps;

	internal LoggedPropertyResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps, LogRecordFilter.PropertyCondition property)
		: base(expectationBuilder, returnValue, filter, quantifier, repeatedCheckOptions, steps)
	{
		_quantifier = quantifier;
		_steps = steps;
		_property = property;
	}

	/// <summary>
	///     Only counts records whose property has a value equal to the <paramref name="expected" /> value.
	/// </summary>
	/// <remarks>
	///     The <see cref="FakeLogger" /> stores the values of the properties as strings in the invariant culture, e.g.
	///     <c>"1.5"</c> for the <see cref="double" /> 1.5, so the <paramref name="expected" /> value is compared with
	///     this string. The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c>
	///     or <c>IgnoringCase()</c>.
	/// </remarks>
	/// <exception cref="System.InvalidOperationException">The value of the property is already specified.</exception>
	public LoggedPropertyValueResult WithValue(string? expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, _quantifier, RepeatedCheckOptions, _steps,
			_property.WithValue(expected));
}
