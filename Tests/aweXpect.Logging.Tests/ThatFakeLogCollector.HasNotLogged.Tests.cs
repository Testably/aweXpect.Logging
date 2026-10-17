using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasNotLogged
	{
		public sealed class Tests
		{
			[Fact]
			public async Task OrHigher_WhenHigherLevelWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning).OrHigher();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning or higher,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Retry limit reached"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Error] "Retry limit reached"
					             ]
					             """);
			}

			[Fact]
			public async Task OrHigher_WhenOnlyLowerLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogDebug("Connecting");
				logger.LogInformation("Connected");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning).OrHigher();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenCombinedWithHasLogged_ShouldVerifyBoth()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).And.HasNotLogged(LogLevel.Error);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Information at least once and has not logged a record with level Error,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Retry limit reached"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Error] "Retry limit reached"
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
						.Whose(x => x.Collectors, c => c.All().ComplyWith(item => item.HasNotLogged(LogLevel.Warning)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that new { Collectors = subject, }
					             whose Collectors have not logged a record with level Warning for all items,
					             *
					             """).AsWildcard()
					.Because("the items are the plural subject of the expectation");
			}

			[Fact]
			public async Task WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning);

				await That(Act).DoesNotThrow()
					.Because("the level must match exactly, so a more severe record does not count");
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
					=> await That(subject).HasNotLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning,
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

			[Fact]
			public async Task WithException_WhenExceptionIsOfOtherType_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new InvalidOperationException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Error).WithException<TimeoutException>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WithException_WhenExceptionIsOfType_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				TimeoutException exception = new();
				logger.LogError(new InvalidOperationException(), "Retry 1");
				logger.LogCritical(exception, "Retry 2");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Error).OrHigher().WithException<TimeoutException>();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Error or higher and exception of type TimeoutException,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Critical] "Retry 2" (TimeoutException)
					             ]

					             Records:
					             [
					               [Error] "Retry 1" (InvalidOperationException),
					               [Critical] "Retry 2" (TimeoutException)
					             ]
					             """);
			}

			[Fact]
			public async Task WithMessage_WhenMessageDiffers_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Connection lost");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning).WithMessage("Retry *").AsWildcard();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WithMessage_WhenMessageMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Connection lost");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning).WithMessage("retry *").AsWildcard()
						.IgnoringCase();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning and message matching "retry *" ignoring case,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Retry 1"
					             ]

					             Records:
					             [
					               [Warning] "Connection lost",
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Fact]
			public async Task WithoutLevel_WhenAnyRecordWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogTrace("Polling");

				async Task Act()
					=> await That(subject).HasNotLogged();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Trace] "Polling"
					             ]

					             Records:
					             [
					               [Trace] "Polling"
					             ]
					             """);
			}

			[Fact]
			public async Task WithoutLevel_WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasNotLogged();

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasNotLogged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once,
					             but it was <null>
					             """)
					.Because("a null collector cannot tell whether a record was logged");
			}

			[Fact]
			public async Task WhenNothingWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasNotLogged(LogLevel.Warning));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once,
					             but it did not log any record
					             """);
			}

			[Fact]
			public async Task WhenRecordWithLevelWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasNotLogged(LogLevel.Warning));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
