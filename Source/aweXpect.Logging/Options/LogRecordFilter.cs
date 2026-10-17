using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Formatting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using static aweXpect.Formatting.Format;

namespace aweXpect.Options;

/// <summary>
///     The conditions a <see cref="FakeLogRecord" /> must meet to count as a match.
/// </summary>
internal sealed class LogRecordFilter(LogLevel? level)
{
	private readonly List<string> _categories = [];
	private readonly List<int> _eventIds = [];
	private readonly List<Type> _exceptionTypes = [];
	private readonly List<(string Expected, StringEqualityOptions Options)> _messages = [];
	private bool _orHigher;

	/// <summary>
	///     Whether the category is compared, so that the failure message lists it for each record.
	/// </summary>
	public bool ComparesCategory => _categories.Count > 0;

	/// <summary>
	///     Whether the event ID is compared, so that the failure message lists it for each record.
	/// </summary>
	public bool ComparesEventId => _eventIds.Count > 0;

	public void AddCategory(string category)
		=> _categories.Add(category);

	public void AddEventId(int eventId)
		=> _eventIds.Add(eventId);

	public void AddException(Type exceptionType)
		=> _exceptionTypes.Add(exceptionType);

	/// <summary>
	///     The category of an <see cref="ILogger{TCategoryName}" /> for the <paramref name="type" />, e.g.
	///     <c>MyApp.Outer.Repository</c> for <c>MyApp.Outer+Repository`1[System.Int32]</c>.
	/// </summary>
	/// <remarks>
	///     Mirrors the category name that <see cref="Logger{T}" /> and <see cref="FakeLogger{T}" /> use: nested types
	///     are separated by a dot, and generic types are named without their arity and type arguments.
	/// </remarks>
	public static string GetCategoryName(Type type)
	{
		string fullName = (type.IsGenericType ? type.GetGenericTypeDefinition() : type).FullName ?? type.Name;
		StringBuilder categoryName = new(fullName.Length);
		bool isArity = false;
		foreach (char character in fullName)
		{
			if (character == '`')
			{
				isArity = true;
			}
			else if (!isArity || !char.IsDigit(character))
			{
				isArity = false;
				categoryName.Append(character == '+' ? '.' : character);
			}
		}

		return categoryName.ToString();
	}

	public StringEqualityOptions AddMessage(string expected)
	{
		StringEqualityOptions options = new(nameof(expected));
		_messages.Add((expected, options));
		return options;
	}

	/// <summary>
	///     Also matches records with a more severe level than the expected one.
	/// </summary>
	public void OrHigher()
		=> _orHigher = true;

	/// <summary>
	///     Rejects an unusable expected message, e.g. an invalid regex, also when no record is compared with it.
	/// </summary>
	public void ValidateExpected()
	{
		foreach ((string expected, StringEqualityOptions options) in _messages)
		{
			options.ValidateExpected(expected);
		}
	}

	public async ValueTask<bool> Matches(FakeLogRecord record)
	{
		if (level is not null && (_orHigher ? record.Level < level : record.Level != level))
		{
			return false;
		}

		if (_categories.Any(category => !string.Equals(category, record.Category, StringComparison.Ordinal)) ||
		    _eventIds.Any(eventId => eventId != record.Id.Id) ||
		    _exceptionTypes.Any(exceptionType => !exceptionType.IsInstanceOfType(record.Exception)))
		{
			return false;
		}

		foreach ((string expected, StringEqualityOptions options) in _messages)
		{
			if (!await options.AreConsideredEqual(record.Message, expected))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Describes a matching record, e.g. <c>a record with level Warning and message matching "Retry*"</c>.
	/// </summary>
	public override string ToString()
	{
		List<string> conditions = [];
		if (level is not null)
		{
			conditions.Add($"level {Formatter.Format(level.Value)}{(_orHigher ? " or higher" : "")}");
		}

		foreach ((string expected, StringEqualityOptions options) in _messages)
		{
			conditions.Add($"message {options.GetExpectation(expected, ExpectationGrammars.None)}");
		}

		foreach (Type exceptionType in _exceptionTypes)
		{
			conditions.Add($"exception of type {Formatter.Format(exceptionType)}");
		}

		foreach (string category in _categories)
		{
			conditions.Add($"category {Formatter.Format(category)}");
		}

		foreach (int eventId in _eventIds)
		{
			conditions.Add($"event ID {eventId}");
		}

		return conditions.Count == 0 ? "a record" : $"a record with {string.Join(" and ", conditions)}";
	}
}
