using System;
using aweXpect.Core;
using aweXpect.Options;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Results;

/// <summary>
///     The result of an expectation that a <see cref="FakeLogCollector" /> did not log a matching record.
///     <para />
///     Allows combining multiple expectations with <see cref="AndOrResult{TType,TThat,TSelf}.And" /> and
///     <see cref="AndOrResult{TType,TThat,TSelf}.Or" />.
/// </summary>
/// <remarks>
///     It offers no quantifiers, as any amount but zero contradicts the expectation; a limited amount of records is
///     verified with <see cref="LoggedResult.AtMost(Times)" /> instead.
/// </remarks>
public class DidNotLogResult : LogRecordFilterResult<DidNotLogResult>
{
	/// <remarks>
	///     Internal, so that the result can take the options of further filters without a breaking change.
	/// </remarks>
	internal DidNotLogResult(ExpectationBuilder expectationBuilder, IThat<FakeLogCollector?> returnValue,
		LogRecordFilter filter, RepeatedCheckOptions repeatedCheckOptions)
		: base(expectationBuilder, returnValue, filter, repeatedCheckOptions)
	{
	}

	/// <inheritdoc cref="LoggedResult.Within(TimeSpan)" />
	public DidNotLogWithinResult Within(TimeSpan timeout)
	{
		RepeatedCheckOptions.Within(timeout);
		return new DidNotLogWithinResult(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions);
	}

	/// <inheritdoc cref="LoggedResult.WithCategory(string)" />
	public DidNotLogCategoryResult WithCategory(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddCategory(expected));

	/// <summary>
	///     Only considers records whose message is equal to the <paramref name="expected" /> message.
	/// </summary>
	/// <remarks>
	///     The match type and the comparison can be changed on the result, e.g. with <c>AsWildcard()</c> or
	///     <c>IgnoringCase()</c>.
	/// </remarks>
	public DidNotLogMessageResult WithMessage(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddMessage(expected));

	/// <summary>
	///     Only considers records whose message template, e.g. <c>Retry {Attempt} of {MaxAttempts}</c>, is equal to the
	///     <paramref name="expected" /> template.
	/// </summary>
	/// <remarks>
	///     Unlike the message, the template does not depend on the values that were logged, so that it identifies the
	///     statement that logged the record. The match type and the comparison can be changed on the result, e.g. with
	///     <c>AsPrefix()</c> or <c>AsWildcard()</c>.
	/// </remarks>
	public DidNotLogMessageTemplateResult WithMessageTemplate(string expected)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddMessageTemplate(expected));

	/// <summary>
	///     Only considers records with a property <paramref name="name" />, e.g. <c>Attempt</c> for the message
	///     template <c>Retry {Attempt}</c>.
	/// </summary>
	/// <remarks>
	///     Its value can be verified on the result with <see cref="DidNotLogPropertyResult.WithValue(string)" />.
	/// </remarks>
	public DidNotLogPropertyResult WithProperty(string name)
		=> new(ExpectationBuilder, ReturnValue, Filter, RepeatedCheckOptions, Filter.AddProperty(name));
}
