using System;
using aweXpect.Options;

namespace aweXpect.Results;

/// <summary>
///     A step of <see cref="LoggedResult.Then(Func{LogRecordStep, LogRecordStepResult}[])" />, which allows
///     narrowing down the record that is expected for it.
/// </summary>
/// <remarks>
///     It offers the filters of a <see cref="LoggedResult" />, but neither quantifiers nor a timeout, as these apply
///     to the whole order.
/// </remarks>
public class LogRecordStepResult
{
	internal LogRecordStepResult(LogRecordFilter filter)
	{
		Filter = filter;
	}

	internal LogRecordFilter Filter { get; }

	/// <inheritdoc cref="LoggedResult.WithCategory(string)" />
	public LogRecordStepCategoryResult WithCategory(string expected)
		=> new(Filter, Filter.AddCategory(expected));

	/// <inheritdoc cref="LogRecordFilterResult{TSelf}.WithCategory{TCategory}()" />
	public LogRecordStepResult WithCategory<TCategory>()
	{
		Filter.AddCategory(LogRecordFilter.GetCategoryName(typeof(TCategory)));
		return this;
	}

	/// <inheritdoc cref="LogRecordFilterResult{TSelf}.WithEventId(int)" />
	public LogRecordStepResult WithEventId(int eventId)
	{
		Filter.AddEventId(eventId);
		return this;
	}

	/// <inheritdoc cref="LogRecordFilterResult{TSelf}.WithException{TException}()" />
	public LogRecordStepResult WithException<TException>()
		where TException : Exception
	{
		Filter.AddException(typeof(TException));
		return this;
	}

	/// <inheritdoc cref="LoggedResult.WithMessage(string)" />
	public LogRecordStepMessageResult WithMessage(string expected)
		=> new(Filter, Filter.AddMessage(expected));

	/// <inheritdoc cref="LogRecordFilterResult{TSelf}.WithMessageTemplate(string)" />
	public LogRecordStepResult WithMessageTemplate(string expected)
	{
		Filter.AddMessageTemplate(expected);
		return this;
	}

	/// <inheritdoc cref="LoggedResult.WithProperty(string)" />
	public LogRecordStepPropertyResult WithProperty(string name)
		=> new(Filter, Filter.AddProperty(name));
}
