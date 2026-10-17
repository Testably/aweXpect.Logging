using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class OrHigherTests
		{
			[Fact]
			public async Task WhenCombinedWithMessageAndQuantifier_ShouldOnlyCountMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Retry 1");
				logger.LogWarning("Retry 2");
				logger.LogError("Connection lost");
				logger.LogCritical("Retry 3");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).OrHigher()
						.WithMessage("Retry *").AsWildcard()
						.Once();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning or higher and message matching "Retry *" exactly once,
					             but it logged 2 such records

					             Records:
					             [
					               [Information] "Retry 1",
					               [Warning] "Retry 2",
					               [Error] "Connection lost",
					               [Critical] "Retry 3"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenNegated_AndHigherLevelWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged(LogLevel.Warning).OrHigher());

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
					               [Error] "Retry limit reached"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenNegated_AndOnlyLowerLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogDebug("Connecting");
				logger.LogInformation("Connected");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged(LogLevel.Warning).OrHigher());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenOnlyLowerLevelsWereLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogDebug("Connecting");
				logger.LogInformation("Connected");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).OrHigher();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning or higher at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Debug] "Connecting",
					               [Information] "Connected"
					             ]
					             """);
			}

			[Theory]
			[InlineData(LogLevel.Warning)]
			[InlineData(LogLevel.Error)]
			[InlineData(LogLevel.Critical)]
			public async Task WhenRecordWithSameOrHigherLevelWasLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.Log(level, "Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).OrHigher();

				await That(Act).DoesNotThrow();
			}
		}
	}
}
