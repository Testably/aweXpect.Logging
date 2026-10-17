using System.Collections.Generic;
using System.Linq;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.Extending;
using aweXpect.Formatting;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using static aweXpect.Formatting.Format;

namespace aweXpect;

/// <summary>
///     Expectations on <see cref="FakeLogCollector" /> values.
/// </summary>
public static partial class ThatFakeLogCollector
{
	/// <summary>
	///     Verifies that the subject has logged at least one record with the <paramref name="level" />.
	/// </summary>
	[GuaranteesNotNull]
	public static HasLoggedResult HasLogged(this IThat<FakeLogCollector?> source, LogLevel level)
		=> new(
			source.Get().ExpectationBuilder.AddConstraint(level, static (l, it, grammars) =>
				new HasLoggedConstraint(it, grammars, l)),
			source);

	private sealed class HasLoggedConstraint(string it, ExpectationGrammars grammars, LogLevel level)
		: ConstraintResult.WithNotNullValue<FakeLogCollector?>(it, grammars),
			IValueConstraint<FakeLogCollector?>
	{
		private IReadOnlyList<FakeLogRecord> _records = [];
		private int _matchingCount;

		public ConstraintResult IsMetBy(FakeLogCollector? actual)
		{
			Actual = actual;
			// A snapshot, as the collector can receive further records while the result is evaluated and rendered.
			_records = actual?.GetSnapshot() ?? [];
			_matchingCount = _records.Count(record => record.Level == level);
			Outcome = _matchingCount > 0 ? Outcome.Success : Outcome.Failure;
			return this;
		}

		public override void AppendContexts(ResultContextCollector contexts)
		{
			if (_records.Count > 0)
			{
				IReadOnlyList<FakeLogRecord> records = _records;
				contexts.Add(new ResultContext.SyncCallback("Records",
					() => Formatter.Format(records.Select(record => new FormattedRecord(record)).ToArray(),
						FormattingOptions.MultipleLines)));
			}
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectation(stringBuilder, "has logged", "have logged");

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(It);
			if (_records.Count == 0)
			{
				stringBuilder.Append(" did not log any record");
			}
			else
			{
				stringBuilder.Append(" only logged records with other levels");
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectation(stringBuilder, "has not logged", "have not logged");

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(It).Append(" logged ").Append(_matchingCount)
				.Append(_matchingCount == 1 ? " such record" : " such records");
		}

		private void AppendExpectation(StringBuilder stringBuilder, string singular, string plural)
		{
			stringBuilder.Append(Grammars.Verb(singular, plural)).Append(" a record with level ");
			Formatter.Format(stringBuilder, level);
		}
	}

	/// <summary>
	///     Lists a record by its level and message.
	/// </summary>
	/// <remarks>
	///     A <see cref="FakeLogRecord" /> itself would be listed with all its members, which hides the level and the
	///     message among details like the timestamp.
	/// </remarks>
	private sealed class FormattedRecord(FakeLogRecord record)
	{
		public override string ToString()
			=> $"[{Formatter.Format(record.Level)}] {Formatter.Format(record.Message)}";
	}
}
