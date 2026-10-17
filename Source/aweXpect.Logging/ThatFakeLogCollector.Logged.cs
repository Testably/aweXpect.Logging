using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.EvaluationContext;
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
	///     Verifies that the subject logged a record of any level, by default at least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedResult Logged(this IThat<FakeLogCollector?> source)
	{
		LogRecordFilter filter = new(null);
		Quantifier quantifier = new();
		RepeatedCheckOptions repeatedCheckOptions = new();
		List<LogRecordFilter> steps = [];
		return new LoggedResult(AddLoggedConstraint(source, filter, quantifier, repeatedCheckOptions, steps),
			source, filter, quantifier, repeatedCheckOptions, steps);
	}

	/// <summary>
	///     Verifies that the subject logged a record with the <paramref name="level" />, by default at least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult Logged(this IThat<FakeLogCollector?> source, LogLevel level)
	{
		LogRecordFilter filter = new(level);
		Quantifier quantifier = new();
		RepeatedCheckOptions repeatedCheckOptions = new();
		List<LogRecordFilter> steps = [];
		return new LoggedLevelResult(
			AddLoggedConstraint(source, filter, quantifier, repeatedCheckOptions, steps), source, filter,
			quantifier, repeatedCheckOptions, steps);
	}

	/// <remarks>
	///     For the collector of a <see cref="FakeLogger" />, the constraint is forwarded the collector of the logger,
	///     as the value of the expectation stays the logger.
	/// </remarks>
	private static ExpectationBuilder AddLoggedConstraint(IThat<FakeLogCollector?> source, LogRecordFilter filter,
		Quantifier quantifier, RepeatedCheckOptions repeatedCheckOptions, List<LogRecordFilter> steps)
	{
		ExpectationBuilder expectationBuilder = source.Get().ExpectationBuilder;
		(LogRecordFilter Filter, Quantifier Quantifier, RepeatedCheckOptions RepeatedCheckOptions,
			List<LogRecordFilter> Steps) state = (filter, quantifier, repeatedCheckOptions, steps);
		return source is ThatFakeLogger.CollectorOfLogger
			? expectationBuilder.AddConstraint(state, static (state, it, grammars)
				=> new CollectorOfLoggerConstraint(new LoggedConstraint(it, grammars, state.Filter,
					state.Quantifier, state.RepeatedCheckOptions, state.Steps)))
			: expectationBuilder.AddConstraint(state, static (state, it, grammars)
				=> new LoggedConstraint(it, grammars, state.Filter, state.Quantifier, state.RepeatedCheckOptions,
					state.Steps));
	}

	/// <summary>
	///     Verifies the collector of a <see cref="FakeLogger" /> with the <paramref name="constraint" />.
	/// </summary>
	private sealed class CollectorOfLoggerConstraint(LoggedConstraint constraint)
		: IAsyncContextConstraint<FakeLogger?>,
			IExpectationTextConstraint
	{
		public ValueTask<ConstraintResult> IsMetBy(FakeLogger? actual, IEvaluationContext context,
			CancellationToken cancellationToken)
			=> constraint.IsMetBy(actual?.Collector, context, cancellationToken);

		public ValueTask<ConstraintResult> GetExpectationResult(IEvaluationContext context,
			CancellationToken cancellationToken)
			=> new(constraint);

		public void AppendExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> constraint.AppendExpectation(stringBuilder, indentation);
	}

	/// <remarks>
	///     The <paramref name="steps" /> are added by <see cref="LoggedResult.Then" /> after the constraint, so it
	///     verifies the order of the records instead of their amount, as soon as there are any.
	/// </remarks>
	private sealed class LoggedConstraint(
		string it,
		ExpectationGrammars grammars,
		LogRecordFilter filter,
		Quantifier quantifier,
		RepeatedCheckOptions repeatedCheckOptions,
		List<LogRecordFilter> steps)
		: ConstraintResult.WithNotNullValue<FakeLogCollector?>(it, grammars),
			IAsyncContextConstraint<FakeLogCollector?>
	{
		private bool _isMissingStepLogged;
		private List<FakeLogRecord> _matchingRecords = [];
		private LogRecordFilter? _missingStep;
		private List<FakeLogRecord> _notMatchingRecords = [];
		private IReadOnlyList<FakeLogRecord> _records = [];

		private bool IsOrdered => steps.Count > 0;

		/// <remarks>
		///     With a timeout, the records are read repeatedly until their amount decides the outcome, as for recorded
		///     events: a lower bound stops waiting as soon as it is met, an upper bound as soon as it is exceeded.
		/// </remarks>
		public async ValueTask<ConstraintResult> IsMetBy(FakeLogCollector? actual, IEvaluationContext context,
			CancellationToken cancellationToken)
		{
			if (IsOrdered && !IsAtLeastOnce())
			{
				throw new InvalidOperationException("Then cannot be combined with a quantifier.");
			}

			filter.ValidateExpected();
			steps.ForEach(step => step.ValidateExpected());
			Actual = actual;
			if (actual is null)
			{
				await Read([]);
				return this;
			}

			Outcome repeatedOutcome = await repeatedCheckOptions.CheckRepeatedly(async _ =>
			{
				// A snapshot, as the collector can receive further records while the result is evaluated and rendered.
				await Read(actual.GetSnapshot());
				return IsOrdered ? _missingStep is null : quantifier.Check(_matchingRecords.Count, false) is not null;
			}, context);
			if (repeatedOutcome == Outcome.Undecided)
			{
				Outcome = Outcome.Undecided;
				return this;
			}

			Outcome = IsMetByTheRecords() ? Outcome.Success : Outcome.Failure;
			return this;
		}

		/// <summary>
		///     Whether the quantifier is equivalent to the default, i.e. at least once, which the order of the records
		///     implies.
		/// </summary>
		private bool IsAtLeastOnce()
			=> quantifier.Check(0, true) != true && quantifier.Check(1, true) == true &&
			   quantifier.Check(int.MaxValue, true) == true;

		private bool IsMetByTheRecords()
		{
			if (IsOrdered)
			{
				return _missingStep is null;
			}

			return quantifier.Check(_matchingRecords.Count, true) == true;
		}

		private async ValueTask Read(IReadOnlyList<FakeLogRecord> records)
		{
			List<FakeLogRecord> matchingRecords = [];
			List<FakeLogRecord> notMatchingRecords = [];
			foreach (FakeLogRecord record in records)
			{
				(await filter.Matches(record) ? matchingRecords : notMatchingRecords).Add(record);
			}

			_records = records;
			_matchingRecords = matchingRecords;
			_notMatchingRecords = notMatchingRecords;
			if (IsOrdered)
			{
				await ReadOrder(records);
			}
		}

		/// <summary>
		///     Determines the first step without a matching record after the one of the previous step.
		/// </summary>
		/// <remarks>
		///     Each step takes the first matching record after the one of the previous step, as a later record could
		///     only leave fewer records for the following steps.
		/// </remarks>
		private async ValueTask ReadOrder(IReadOnlyList<FakeLogRecord> records)
		{
			int previousPosition = -1;
			for (int step = 0; step <= steps.Count; step++)
			{
				LogRecordFilter stepFilter = step == 0 ? filter : steps[step - 1];
				int position = await FindFirst(stepFilter, records, previousPosition + 1);
				if (position < 0)
				{
					_missingStep = stepFilter;
					_isMissingStepLogged = step > 0 && await FindFirst(stepFilter, records, 0) >= 0;
					return;
				}

				previousPosition = position;
			}

			_missingStep = null;
			_isMissingStepLogged = false;
		}

		private static async ValueTask<int> FindFirst(LogRecordFilter stepFilter,
			IReadOnlyList<FakeLogRecord> records, int start)
		{
			for (int position = start; position < records.Count; position++)
			{
				if (await stepFilter.Matches(records[position]))
				{
					return position;
				}
			}

			return -1;
		}

		/// <remarks>
		///     As for the quantified collection expectations in aweXpect, the matching records are listed when the
		///     quantifier only has an upper bound, e.g. <c>AtMost</c> or <c>Never</c>, and the not matching records when
		///     it only has a lower bound above one, e.g. <c>AtLeast(2.Times())</c>, because these are the records that
		///     explain the failure. A negated expectation lists them for its complement.
		/// </remarks>
		/// <remarks>
		///     The order of the records is only shown by all of them, so then no further records are listed.
		/// </remarks>
		public override void AppendContexts(ResultContextCollector contexts)
		{
			if (!IsOrdered)
			{
				AddQuantifierContexts(contexts);
			}

			AddRecords(contexts, "Records", _records);
		}

		private void AddQuantifierContexts(ResultContextCollector contexts)
		{
			if (IsMet(0) && !IsMet(int.MaxValue))
			{
				AddRecords(contexts, "Matching records", _matchingRecords, int.MaxValue);
			}
			else if (!IsMet(0) && !IsMet(1) && IsMet(int.MaxValue))
			{
				AddRecords(contexts, "Not matching records", _notMatchingRecords, int.MaxValue);
			}
		}

		/// <summary>
		///     Whether the <paramref name="amount" /> of matching records meets the expectation, or its negation.
		/// </summary>
		private bool IsMet(int amount)
			=> quantifier.Check(amount, true, IsNegated) ?? IsNegated;

		private void AddRecords(ResultContextCollector contexts, string title,
			IReadOnlyList<FakeLogRecord> records, int priority = 0)
		{
			if (records.Count > 0)
			{
				LogRecordFilter[] filters = [filter, ..steps,];
				contexts.Add(new ResultContext.SyncCallback(title,
					() => Formatter.Format(records.Select(record => new FormattedRecord(record, filters)).ToArray(),
						FormattingOptions.MultipleLines), priority));
			}
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (IsOrdered)
			{
				AppendOrderedExpectation(stringBuilder, indentation, " in order");
			}
			else
			{
				AppendExpectation(stringBuilder, false);
			}
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (IsOrdered)
			{
				AppendOrderedResult(stringBuilder);
			}
			else
			{
				AppendResult(stringBuilder);
			}
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (IsOrdered)
			{
				AppendOrderedExpectation(stringBuilder, indentation, " not in order");
			}
			else
			{
				AppendExpectation(stringBuilder, true);
			}
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (IsOrdered)
			{
				stringBuilder.Append(It).Append(" did");
			}
			else
			{
				AppendResult(stringBuilder);
			}
		}

		/// <remarks>
		///     As for the order of interactions in aweXpect.Mockolate, each step is described on its own line, e.g.
		///     <c>logged a record with level Information, then</c> and <c>a record with level Warning in order</c>.
		/// </remarks>
		private void AppendOrderedExpectation(StringBuilder stringBuilder, string? indentation, string order)
		{
			stringBuilder.Append("logged ").Append(filter);
			foreach (LogRecordFilter step in steps)
			{
				stringBuilder.Append(", then").Append(Environment.NewLine).Append(indentation).Append(step);
			}

			stringBuilder.Append(order).Append(repeatedCheckOptions);
		}

		private void AppendOrderedResult(StringBuilder stringBuilder)
		{
			stringBuilder.Append(It);
			if (_records.Count == 0)
			{
				stringBuilder.Append(" did not log any record");
			}
			else if (_isMissingStepLogged)
			{
				stringBuilder.Append(" logged ").Append(_missingStep).Append(" too early");
			}
			else
			{
				stringBuilder.Append(" did not log ").Append(_missingStep).Append(" at all");
			}
		}

		/// <remarks>
		///     A negated quantifier without a complement is rendered as <c>not …</c>, so the negation is moved to the verb,
		///     e.g. <c>did not log a record with level Warning exactly twice</c>.
		/// </remarks>
		private void AppendExpectation(StringBuilder stringBuilder, bool isNegated)
		{
			const string not = "not ";
			string? times = quantifier.IsNever(isNegated) ? null : quantifier.ToString(isNegated);
			bool hasNot = times is null || times.StartsWith(not, StringComparison.Ordinal);
			stringBuilder.Append(hasNot ? "did not log " : "logged ").Append(filter);
			if (times is not null)
			{
				stringBuilder.Append(' ').Append(hasNot ? times.Substring(not.Length) : times);
			}

			stringBuilder.Append(repeatedCheckOptions);
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
	///     Lists a record by its level, its message and the type of its exception, if any, and by the further details
	///     that the <paramref name="filters" /> compare, e.g. its category.
	/// </summary>
	/// <remarks>
	///     A <see cref="FakeLogRecord" /> itself would be listed with all its members, which hides the level and the
	///     message among details like the timestamp.
	/// </remarks>
	private sealed class FormattedRecord(FakeLogRecord record, LogRecordFilter[] filters)
	{
		public override string ToString()
		{
			List<string> details = [];
			if (record.Exception is not null)
			{
				details.Add(Formatter.Format(record.Exception.GetType()));
			}

			details.AddRange(filters.SelectMany(filter => filter.GetComparedDetails(record)).Distinct());
			string formattedRecord = $"[{Formatter.Format(record.Level)}] {Formatter.Format(record.Message)}";
			return details.Count == 0 ? formattedRecord : $"{formattedRecord} ({string.Join(", ", details)})";
		}
	}
}
