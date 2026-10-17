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
	/// <summary>
	///     The key of the message template in the structured state of a record.
	/// </summary>
	private const string OriginalFormat = "{OriginalFormat}";

	private readonly List<string> _categories = [];
	private readonly List<int> _eventIds = [];
	private readonly List<Type> _exceptionTypes = [];
	private readonly List<string> _messageTemplates = [];
	private readonly List<(string Expected, StringEqualityOptions Options)> _messages = [];
	private readonly List<PropertyCondition> _properties = [];
	private bool _orHigher;

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

	public void AddMessageTemplate(string expected)
		=> _messageTemplates.Add(expected);

	public PropertyCondition AddProperty(string name)
	{
		PropertyCondition property = new(name);
		_properties.Add(property);
		return property;
	}

	/// <summary>
	///     The details of the <paramref name="record" /> that are compared beyond its level, message and exception, so
	///     that the failure message shows why a record did not match, e.g. <c>property Attempt "2"</c>.
	/// </summary>
	public IEnumerable<string> GetComparedDetails(FakeLogRecord record)
	{
		if (_categories.Count > 0)
		{
			yield return $"category {Formatter.Format(record.Category)}";
		}

		if (_eventIds.Count > 0)
		{
			yield return $"event ID {record.Id.Id}";
		}

		if (_messageTemplates.Count == 0 && _properties.Count == 0)
		{
			yield break;
		}

		if (GetStructuredState(record) is not { } state)
		{
			yield return "no structured state";
			yield break;
		}

		if (_messageTemplates.Count > 0)
		{
			yield return DescribeValue(state, OriginalFormat, "message template");
		}

		foreach (string name in _properties.Select(property => property.Name).Distinct(StringComparer.Ordinal))
		{
			yield return DescribeValue(state, name, $"property {name}");
		}
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

		foreach (PropertyCondition property in _properties)
		{
			if (property.Value is { } value)
			{
				value.Options.ValidateExpected(value.Expected);
			}
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

		if ((_messageTemplates.Count > 0 || _properties.Count > 0) &&
		    (GetStructuredState(record) is not { } state ||
		     _messageTemplates.Any(template => !HasValue(state, OriginalFormat, template)) ||
		     !await HasMatchingProperties(state)))
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

		foreach (string messageTemplate in _messageTemplates)
		{
			conditions.Add($"message template equal to {Formatter.Format(messageTemplate)}");
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

		foreach (PropertyCondition property in _properties)
		{
			string condition = $"property {property.Name}";
			if (property.Value is { } value)
			{
				condition += $" with value {value.Options.GetExpectation(value.Expected, ExpectationGrammars.None)}";
			}

			conditions.Add(condition);
		}

		return conditions.Count == 0 ? "a record" : $"a record with {string.Join(" and ", conditions)}";
	}

	private static string DescribeValue(IReadOnlyList<KeyValuePair<string, string?>> state, string key, string label)
	{
		KeyValuePair<string, string?> entry = FindEntry(state, key);
		return entry.Key is null ? $"no {label}" : $"{label} {Formatter.Format(entry.Value)}";
	}

	/// <summary>
	///     The entry with the <paramref name="key" />, or the default entry without a key, if there is none.
	/// </summary>
	private static KeyValuePair<string, string?> FindEntry(IReadOnlyList<KeyValuePair<string, string?>> state,
		string key)
		=> state.FirstOrDefault(entry => string.Equals(entry.Key, key, StringComparison.Ordinal));

	/// <summary>
	///     The properties and the message template of the <paramref name="record" />, which the
	///     <see cref="FakeLogger" /> stores as strings.
	/// </summary>
	/// <remarks>
	///     <see cref="FakeLogRecord.StructuredState" /> is not used, as it throws for a state that is no list of
	///     properties, e.g. a custom state object, which the <see cref="FakeLogger" /> stores as its string instead.
	/// </remarks>
	private static IReadOnlyList<KeyValuePair<string, string?>>? GetStructuredState(FakeLogRecord record)
		=> record.State as IReadOnlyList<KeyValuePair<string, string?>>;

	private async ValueTask<bool> HasMatchingProperties(IReadOnlyList<KeyValuePair<string, string?>> state)
	{
		foreach (PropertyCondition property in _properties)
		{
			KeyValuePair<string, string?> entry = FindEntry(state, property.Name);
			if (entry.Key is null ||
			    (property.Value is { } value && !await value.Options.AreConsideredEqual(entry.Value, value.Expected)))
			{
				return false;
			}
		}

		return true;
	}

	private static bool HasValue(IReadOnlyList<KeyValuePair<string, string?>> state, string key, string? expected)
	{
		KeyValuePair<string, string?> entry = FindEntry(state, key);
		return entry.Key is not null && string.Equals(entry.Value, expected, StringComparison.Ordinal);
	}

	/// <summary>
	///     A property that a record must have, optionally with a matching value.
	/// </summary>
	internal sealed class PropertyCondition(string name)
	{
		public string Name { get; } = name;

		public (string? Expected, StringEqualityOptions Options)? Value { get; private set; }

		/// <exception cref="InvalidOperationException">The value is already specified.</exception>
		public StringEqualityOptions WithValue(string? expected)
		{
			if (Value is not null)
			{
				throw new InvalidOperationException($"{nameof(WithValue)} cannot be specified more than once.");
			}

			StringEqualityOptions options = new(nameof(expected));
			Value = (expected, options);
			return options;
		}
	}
}
