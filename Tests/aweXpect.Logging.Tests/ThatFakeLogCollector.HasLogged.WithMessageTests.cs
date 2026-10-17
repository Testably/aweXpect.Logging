using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithMessageTests
		{
			[Fact]
			public async Task AsPrefix_WhenMessageStartsWithExpected_ShouldSucceed()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry").AsPrefix();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task AsRegex_WhenPatternDoesNotMatch_ShouldFail()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("^Retry [0-9]+$").AsRegex();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and message matching regex "^Retry [0-9]+$" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry limit reached"
					             ]
					             """);
			}

			[Fact]
			public async Task AsRegex_WhenPatternIsInvalid_ShouldThrowArgumentException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("(").AsRegex();

				await That(Act).Throws<ArgumentException>()
					.Because("an unusable pattern is rejected even when no record is compared with it");
			}

			[Fact]
			public async Task AsWildcard_WhenPatternMatches_ShouldSucceed()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit 3 reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry limit * reached").AsWildcard();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task IgnoringCase_WhenMessageDiffersInCase_ShouldSucceed()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("RETRY LIMIT REACHED").IgnoringCase();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenLevelDiffers_ShouldFail()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Information, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry limit reached");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and message equal to "Retry limit reached" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Retry limit reached"
					             ]
					             """)
					.Because("the record must match the level and the message");
			}

			[Fact]
			public async Task WhenMessageDiffers_ShouldFail()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("retry limit reached");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and message equal to "retry limit reached" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry limit reached"
					             ]
					             """)
					.Because("the message is compared exactly by default");
			}

			[Fact]
			public async Task WhenMessageIsEqual_ShouldSucceed()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry limit reached");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNegated_AndMessageMatches_ShouldFail()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.HasLogged(LogLevel.Warning).WithMessage("Retry *").AsWildcard());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning and message matching "Retry *",
					             but it logged 1 such record

					             Records:
					             [
					               [Warning] "Retry limit reached"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenOptionIsSpecified_ShouldIncludeItInTheExpectation()
			{
				FakeLogCollector subject = WithRecord(LogLevel.Warning, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry").IgnoringCase();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and message equal to "Retry" ignoring case at least once,
					             *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenQuantified_ShouldOnlyCountMatchingMessages()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogWarning("Connection lost");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithMessage("Retry *").AsWildcard().Once();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and message matching "Retry *" exactly once,
					             but it logged 2 such records

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Warning] "Connection lost",
					               [Warning] "Retry 2"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenSpecifiedTwice_ShouldRequireBothMessagesToMatch()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry limit reached");
				logger.LogWarning("Retry scheduled");
				logger.LogWarning("Timeout reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning)
						.WithMessage("Retry").AsPrefix()
						.WithMessage("reached").AsSuffix()
						.Once();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutLevel_ShouldOnlyCompareTheMessage()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Retry limit reached");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged().WithMessage("Retry limit reached").Once();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with message equal to "Retry limit reached" exactly once,
					             but it logged 2 such records

					             Records:
					             [
					               [Information] "Retry limit reached",
					               [Error] "Retry limit reached"
					             ]
					             """);
			}

			private static FakeLogCollector WithRecord(LogLevel level, string message)
			{
				FakeLogCollector collector = new();
				new FakeLogger(collector).Log(level, "{Message}", message);
				return collector;
			}
		}
	}
}
