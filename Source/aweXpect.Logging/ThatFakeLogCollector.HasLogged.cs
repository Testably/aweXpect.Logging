using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
	///     Verifies that the subject has logged a record of any level, by default at least once.
	/// </summary>
	[GuaranteesNotNull]
	public static HasLoggedResult HasLogged(this IThat<FakeLogCollector?> source)
	{
		LogRecordFilter filter = new(null);
		Quantifier quantifier = new();
		return new HasLoggedResult(AddHasLoggedConstraint(source, filter, quantifier), source, filter, quantifier);
	}

	/// <summary>
	///     Verifies that the subject has logged a record with the <paramref name="level" />, by default at least once.
	/// </summary>
	[GuaranteesNotNull]
	public static HasLoggedLevelResult HasLogged(this IThat<FakeLogCollector?> source, LogLevel level)
	{
		LogRecordFilter filter = new(level);
		Quantifier quantifier = new();
		return new HasLoggedLevelResult(AddHasLoggedConstraint(source, filter, quantifier), source, filter,
			quantifier);
	}

	private static ExpectationBuilder AddHasLoggedConstraint(IThat<FakeLogCollector?> source, LogRecordFilter filter,
		Quantifier quantifier)
		=> source.Get().ExpectationBuilder.AddConstraint((Filter: filter, Quantifier: quantifier),
			static (state, it, grammars) => new HasLoggedConstraint(it, grammars, state.Filter, state.Quantifier));

	private sealed class HasLoggedConstraint(
		string it,
		ExpectationGrammars grammars,
		LogRecordFilter filter,
		Quantifier quantifier)
		: ConstraintResult.WithNotNullValue<FakeLogCollector?>(it, grammars),
			IAsyncConstraint<FakeLogCollector?>
	{
		private List<FakeLogRecord> _matchingRecords = [];
		private List<FakeLogRecord> _notMatchingRecords = [];
		private IReadOnlyList<FakeLogRecord> _records = [];

		public async ValueTask<ConstraintResult> IsMetBy(FakeLogCollector? actual, CancellationToken cancellationToken)
		{
			filter.ValidateExpected();
			Actual = actual;
			// A snapshot, as the collector can receive further records while the result is evaluated and rendered.
			_records = actual?.GetSnapshot() ?? [];
			_matchingRecords = [];
			_notMatchingRecords = [];
			foreach (FakeLogRecord record in _records)
			{
				(await filter.Matches(record) ? _matchingRecords : _notMatchingRecords).Add(record);
			}

			Outcome = quantifier.Check(_matchingRecords.Count, true) == true ? Outcome.Success : Outcome.Failure;
			return this;
		}

		/// <remarks>
		///     As for the quantified collection expectations in aweXpect, the matching records are listed when the
		///     quantifier only has an upper bound, e.g. <c>AtMost</c> or <c>Never</c>, and the not matching records when
		///     it only has a lower bound above one, e.g. <c>AtLeast(2.Times())</c>, because these are the records that
		///     explain the failure. A negated expectation lists them for its complement.
		/// </remarks>
		public override void AppendContexts(ResultContextCollector contexts)
		{
			if (IsMet(0) && !IsMet(int.MaxValue))
			{
				AddRecords(contexts, "Matching records", _matchingRecords, int.MaxValue);
			}
			else if (!IsMet(0) && !IsMet(1) && IsMet(int.MaxValue))
			{
				AddRecords(contexts, "Not matching records", _notMatchingRecords, int.MaxValue);
			}

			AddRecords(contexts, "Records", _records);
		}

		/// <summary>
		///     Whether the <paramref name="amount" /> of matching records meets the expectation, or its negation.
		/// </summary>
		private bool IsMet(int amount)
			=> quantifier.Check(amount, true, IsNegated) ?? IsNegated;

		private static void AddRecords(ResultContextCollector contexts, string title,
			IReadOnlyList<FakeLogRecord> records, int priority = 0)
		{
			if (records.Count > 0)
			{
				contexts.Add(new ResultContext.SyncCallback(title,
					() => Formatter.Format(records.Select(record => new FormattedRecord(record)).ToArray(),
						FormattingOptions.MultipleLines), priority));
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
				.Append(' ').Append(filter);
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
			else if (_matchingRecords.Count == 0)
			{
				stringBuilder.Append(" did not log any such record");
			}
			else
			{
				stringBuilder.Append(" logged ").Append(_matchingRecords.Count)
					.Append(_matchingRecords.Count == 1 ? " such record" : " such records");
			}
		}
	}

	/// <summary>
	///     Lists a record by its level, its message and the type of its exception, if any.
	/// </summary>
	/// <remarks>
	///     A <see cref="FakeLogRecord" /> itself would be listed with all its members, which hides the level and the
	///     message among details like the timestamp.
	/// </remarks>
	private sealed class FormattedRecord(FakeLogRecord record)
	{
		public override string ToString()
			=> record.Exception is null
				? $"[{Formatter.Format(record.Level)}] {Formatter.Format(record.Message)}"
				: $"[{Formatter.Format(record.Level)}] {Formatter.Format(record.Message)} ({Formatter.Format(record.Exception.GetType())})";
	}
}
