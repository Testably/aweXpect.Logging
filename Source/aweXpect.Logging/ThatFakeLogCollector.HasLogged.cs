using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.Extending;
using aweXpect.Formatting;
using aweXpect.Options;
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
	///     Verifies that the subject has logged a record with the <paramref name="level" />, by default at least once.
	/// </summary>
	[GuaranteesNotNull]
	public static HasLoggedResult HasLogged(this IThat<FakeLogCollector?> source, LogLevel level)
	{
		Quantifier quantifier = new();
		return new HasLoggedResult(
			source.Get().ExpectationBuilder.AddConstraint((Level: level, Quantifier: quantifier),
				static (state, it, grammars) => new HasLoggedConstraint(it, grammars, state.Level, state.Quantifier)),
			source,
			quantifier);
	}

	private sealed class HasLoggedConstraint(
		string it,
		ExpectationGrammars grammars,
		LogLevel level,
		Quantifier quantifier)
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
			Outcome = quantifier.Check(_matchingCount, true) == true ? Outcome.Success : Outcome.Failure;
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
			=> AppendExpectation(stringBuilder, false);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> AppendResult(stringBuilder);

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> AppendExpectation(stringBuilder, true);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> AppendResult(stringBuilder);

		/// <remarks>
		///     A negated quantifier without a complement is rendered as <c>not …</c>, so the negation is moved to the verb,
		///     e.g. <c>has not logged a record with level Warning exactly twice</c>.
		/// </remarks>
		private void AppendExpectation(StringBuilder stringBuilder, bool isNegated)
		{
			const string not = "not ";
			string? times = quantifier.IsNever(isNegated) ? null : quantifier.ToString(isNegated);
			bool hasNot = times is null || times.StartsWith(not, StringComparison.Ordinal);
			stringBuilder.Append(hasNot
					? Grammars.Verb("has not logged", "have not logged")
					: Grammars.Verb("has logged", "have logged"))
				.Append(" a record with level ");
			Formatter.Format(stringBuilder, level);
			if (times is not null)
			{
				stringBuilder.Append(' ').Append(hasNot ? times.Substring(not.Length) : times);
			}
		}

		private void AppendResult(StringBuilder stringBuilder)
		{
			stringBuilder.Append(It);
			if (_records.Count == 0)
			{
				stringBuilder.Append(" did not log any record");
			}
			else if (_matchingCount == 0)
			{
				stringBuilder.Append(" only logged records with other levels");
			}
			else
			{
				stringBuilder.Append(" logged ").Append(_matchingCount)
					.Append(_matchingCount == 1 ? " such record" : " such records");
			}
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
