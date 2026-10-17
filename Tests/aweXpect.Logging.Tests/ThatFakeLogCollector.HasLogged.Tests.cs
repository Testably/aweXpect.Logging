using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed class HasLogged
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenCombinedWithAnd_ShouldListTheRecordsOnce()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogInformation("Starting");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).And.HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Information and has logged a record with level Warning,
					             but it only logged records with other levels

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
						.Whose(x => x.Collectors, c => c.All().ComplyWith(item => item.HasLogged(LogLevel.Warning)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that new { Collectors = subject, }
					             whose Collectors have logged a record with level Warning for all items,
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
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning,
					             but it only logged records with other levels

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
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              has logged a record with level Warning,
					              but it only logged records with other levels

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
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning,
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
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning,
					             but it only logged records with other levels

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
					=> await That(subject).HasLogged(level);

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, string, Exception?> LogErrorMessage =
				LoggerMessage.Define<string>(LogLevel.Error, new EventId(2), "{Message}");

			private static readonly Action<ILogger, int, Exception?> LogNumberedMessage =
				LoggerMessage.Define<int>(LogLevel.Information, new EventId(1), "Message {Number}");
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning,
					             but it was <null>
					             """)
					.Because("a null collector cannot tell whether a record was logged");
			}

			[Fact]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogInformation("Starting");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged(LogLevel.Warning));

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
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning,
					             but it logged 2 such records

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
