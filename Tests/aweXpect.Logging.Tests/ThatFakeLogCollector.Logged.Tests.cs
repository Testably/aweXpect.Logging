using System.Collections.Generic;
using aweXpect.Core;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenAnyLevelIsExpected_AndNothingWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record at least once,
					             but it did not log any record
					             """);
			}

			[Theory]
			[InlineData(LogLevel.Trace)]
			[InlineData(LogLevel.Critical)]
			public async Task WhenAnyLevelIsExpected_AndRecordWasLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).Log(level, "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenCombinedWithAnd_ShouldListTheRecordsOnce()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogInformation("Starting");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information).And.Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information at least once and logged a record with level Warning at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Starting"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenItemsArePlural_ShouldUsePluralVerb()
			{
				FakeLogCollector[] subject = [new(), new(),];
				new FakeLogger(subject[0]).LogWarning("Retry limit reached");

				async Task Act()
					=> await That(new { Collectors = subject, })
						.Whose(x => x.Collectors, c => c.All().ComplyWith(item => item.Logged(LogLevel.Warning)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that new { Collectors = subject, }
					             whose Collectors logged a record with level Warning at least once for all items,
					             *
					             """).AsWildcard()
					.Because("the items are the plural subject of the expectation");
			}

			[Fact]
			public async Task WhenManyRecordsWereLogged_ShouldListOnlyTheFirstRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				for (int i = 1; i <= 12; i++)
				{
					LogNumberedMessage(logger, i, null);
				}

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Message 1",
					               [Information] "Message 2",
					               [Information] "Message 3",
					               [Information] "Message 4",
					               [Information] "Message 5",
					               [Information] "Message 6",
					               [Information] "Message 7",
					               [Information] "Message 8",
					               [Information] "Message 9",
					               [Information] "Message 10",
					               (… and 2 more)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenMessageIsLong_ShouldShortenIt()
			{
				FakeLogCollector subject = new();
				LogErrorMessage(new FakeLogger(subject), new string('a', 120), null);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              logged a record with level Warning at least once,
					              but it did not log any such record

					              Records:
					              [
					                [Error] "{new string('a', 100)}…"
					              ]
					              """);
			}

			[Fact]
			public async Task WhenNothingWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it did not log any record
					             """);
			}

			[Fact]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Starting",
					               [Error] "Retry limit reached"
					             ]
					             """)
					.Because("the level must match exactly, so a more severe record does not count");
			}

			[Theory]
			[InlineData(LogLevel.Trace)]
			[InlineData(LogLevel.Debug)]
			[InlineData(LogLevel.Information)]
			[InlineData(LogLevel.Warning)]
			[InlineData(LogLevel.Error)]
			[InlineData(LogLevel.Critical)]
			public async Task WhenRecordWithLevelWasLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.Log(level, "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(level);

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, string, Exception?> LogErrorMessage =
				LoggerMessage.Define<string>(LogLevel.Error, new EventId(2), "{Message}");

			private static readonly Action<ILogger, int, Exception?> LogNumberedMessage =
				LoggerMessage.Define<int>(LogLevel.Information, new EventId(1), "Message {Number}");
		}

		public sealed class QuantifierTests
		{
			[Fact]
			public async Task WhenCombinedWithAnd_ShouldListBothQuantifiers()
			{
				FakeLogCollector subject = WithWarnings(1);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Once()
						.And.Logged(LogLevel.Information).Never();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning exactly once and did not log a record with level Information,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Information] "Starting"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry"
					             ]
					             """);
			}

			[Theory]
			[MemberData(nameof(Quantifiers))]
			public async Task WhenCountDoesNotMatch_ShouldFail(string quantifier)
			{
				(Func<LoggedResult, LoggedCountResult> quantify, _, int failingCount, string expectation) =
					Cases[quantifier];
				FakeLogCollector subject = WithWarnings(failingCount);

				async Task Act()
					=> await quantify(That(subject).Logged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              {expectation},
					              but it logged {failingCount} such record{(failingCount == 1 ? "" : "s")}

					              *Records:
					              *
					              """).AsWildcard();
			}

			[Theory]
			[MemberData(nameof(Quantifiers))]
			public async Task WhenCountMatches_ShouldSucceed(string quantifier)
			{
				(Func<LoggedResult, LoggedCountResult> quantify, int passingCount, _, _) = Cases[quantifier];
				FakeLogCollector subject = WithWarnings(passingCount);

				async Task Act()
					=> await quantify(That(subject).Logged(LogLevel.Warning));

				await That(Act).DoesNotThrow();
			}

			private static readonly Dictionary<string, (Func<LoggedResult, LoggedCountResult> Quantify,
				int PassingCount, int FailingCount, string Expectation)> Cases = new()
			{
				["AtLeast(2.Times())"] = (r => r.AtLeast(2.Times()), 2, 1,
					"logged a record with level Warning at least twice"),
				["AtLeast().Twice()"] = (r => r.AtLeast().Twice(), 3, 1,
					"logged a record with level Warning at least twice"),
				["AtMost(1.Times())"] = (r => r.AtMost(1.Times()), 0, 2,
					"logged a record with level Warning at most once"),
				["AtMost().Once()"] = (r => r.AtMost().Once(), 1, 2,
					"logged a record with level Warning at most once"),
				["Between(1).And(2.Times())"] = (r => r.Between(1).And(2.Times()), 2, 3,
					"logged a record with level Warning between 1 and 2 times"),
				["Exactly(3.Times())"] = (r => r.Exactly(3.Times()), 3, 2,
					"logged a record with level Warning exactly 3 times"),
				["LessThan(2.Times())"] = (r => r.LessThan(2.Times()), 1, 2,
					"logged a record with level Warning fewer than twice"),
				["LessThan().Twice()"] = (r => r.LessThan().Twice(), 0, 2,
					"logged a record with level Warning fewer than twice"),
				["MoreThan(1.Times())"] = (r => r.MoreThan(1.Times()), 2, 1,
					"logged a record with level Warning more than once"),
				["MoreThan().Once()"] = (r => r.MoreThan().Once(), 3, 1,
					"logged a record with level Warning more than once"),
				["Never()"] = (r => r.Never(), 0, 1,
					"did not log a record with level Warning"),
				["Once()"] = (r => r.Once(), 1, 2,
					"logged a record with level Warning exactly once"),
				["Twice()"] = (r => r.Twice(), 2, 1,
					"logged a record with level Warning exactly twice"),
			};

			public static TheoryData<string> Quantifiers
			{
				get
				{
					TheoryData<string> quantifiers = new();
					foreach (string quantifier in Cases.Keys)
					{
						quantifiers.Add(quantifier);
					}

					return quantifiers;
				}
			}

			private static FakeLogCollector WithWarnings(int count)
			{
				FakeLogCollector collector = new();
				FakeLogger logger = new(collector);
				logger.LogInformation("Starting");
				for (int i = 0; i < count; i++)
				{
					logger.LogWarning("Retry");
				}

				return collector;
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning,
					             but it was <null>
					             """)
					.Because("a null collector cannot tell whether a record was logged");
			}

			[Fact]
			public async Task WhenNeverIsNegated_AndNothingWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning).Never());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it did not log any record
					             """);
			}

			[Fact]
			public async Task WhenNegatedQuantifierHasComplement_ShouldUseComplement()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning).AtLeast(2.Times()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning fewer than twice,
					             but it logged 2 such records

					             Matching records:
					             [
					               [Warning] "Retry 1",
					               [Warning] "Retry 2"
					             ]

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Warning] "Retry 2"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenNegatedQuantifierHasNoComplement_ShouldNegateVerb()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning).Twice());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning exactly twice,
					             but it logged 2 such records

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Warning] "Retry 2"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenNegatedQuantifierIsNotMet_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning).Twice());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogInformation("Starting");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenRecordsWithLevelWereLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning,
					             but it logged 2 such records

					             Matching records:
					             [
					               [Warning] "Retry 1",
					               [Warning] "Retry 2"
					             ]

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Information] "Starting",
					               [Warning] "Retry 2"
					             ]
					             """);
			}
		}
	}
}
