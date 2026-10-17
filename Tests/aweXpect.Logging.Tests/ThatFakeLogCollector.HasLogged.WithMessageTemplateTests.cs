using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithMessageTemplateTests
		{
			[Fact]
			public async Task WhenCombinedWithProperty_ShouldListBothDetails()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 2, 5, null);

				async Task Act()
					=> await That(subject).HasLogged()
						.WithMessageTemplate("Retry {Attempt} of {MaxAttempts}")
						.WithProperty("Attempt", "3");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with message template equal to "Retry {Attempt} of {MaxAttempts}" and property Attempt equal to "3" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 2 of 5" (message template "Retry {Attempt} of {MaxAttempts}", property Attempt "2")
					             ]
					             """);
			}

			[Fact]
			public async Task WhenStateIsNotStructured_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				CustomState state = new();
				EventId eventId = new(7);
				logger.Log(LogLevel.Information, eventId, state, null, static (_, _) => "Custom state");

				async Task Act()
					=> await That(subject).HasLogged().WithMessageTemplate("Custom state");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with message template equal to "Custom state" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Custom state" (no structured state)
					             ]
					             """)
					.Because("a custom state object has no message template");
			}

			[Fact]
			public async Task WhenTemplateDiffers_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged().WithMessageTemplate("Retry {Attempt}");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with message template equal to "Retry {Attempt}" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}"),
					               [Warning] "Retry limit reached" (message template "Retry limit reached")
					             ]
					             """)
					.Because("the template is compared exactly");
			}

			[Fact]
			public async Task WhenTemplateMatches_ShouldIgnoreTheValues()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				LogRetry(logger, 2, 5, null);
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning)
						.WithMessageTemplate("Retry {Attempt} of {MaxAttempts}")
						.Twice();

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, int, int, Exception?> LogRetry =
				LoggerMessage.Define<int, int>(LogLevel.Warning, new EventId(4), "Retry {Attempt} of {MaxAttempts}");

			private sealed class CustomState;
		}
	}
}
