using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> has not logged a record with a property,
///     which in addition to the options of a <see cref="HasNotLoggedResult" /> allows verifying the value of the
///     property.
/// </summary>
public sealed class HasNotLoggedPropertyResult : HasNotLoggedResult
{
	private readonly LogRecordFilter.PropertyCondition _property;

	internal HasNotLoggedPropertyResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions, LogRecordFilter.PropertyCondition property)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
		_property = property;
	}

	/// <summary>
	///     Only considers records whose property has a value equal to the <paramref name="expected" /> value.
	/// </summary>
	/// <remarks>
	///     The <see cref="FakeLogger" /> stores the values of the properties as strings in the invariant culture, e.g.
	///     <c>"1.5"</c> for the <see cref="double" /> 1.5, so the <paramref name="expected" /> value is compared with
	///     this string. The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c>
	///     or <c>IgnoringCase()</c>.
	/// </remarks>
	/// <exception cref="System.InvalidOperationException">The value of the property is already specified.</exception>
	public HasNotLoggedPropertyValueResult WithValue(string? expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, _property.WithValue(expected));
}
