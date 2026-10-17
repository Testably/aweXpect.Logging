using System.Collections.Generic;
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
	private readonly List<(string Expected, StringEqualityOptions Options)> _messages = [];
	private bool _orHigher;

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

		return conditions.Count == 0 ? "a record" : $"a record with {string.Join(" and ", conditions)}";
	}
}
