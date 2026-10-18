using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class WithMessageTemplateTests
		{
			[Test]
			public async Task AsPrefix_WhenTemplateDoesNotStartWithExpected_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("Retrying").AsPrefix();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "Retrying" as prefix at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}")
					             ]
					             """);
			}

			[Test]
			public async Task AsPrefix_WhenTemplateStartsWithExpected_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				LogRetryForOrder(logger, 2, 5, "ORD-1", null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("Retry {Attempt} of {MaxAttempts}").AsPrefix()
						.Twice();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task AsRegex_WhenPatternIsInvalid_ShouldThrowArgumentException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("(").AsRegex();

				await That(Act).Throws<ArgumentException>()
					.WithMessage("The 'expected' regex pattern is invalid: *").AsWildcard()
					.Because("an unusable pattern is rejected even when no record is compared with it");
			}

			[Test]
			public async Task AsRegex_WhenTemplateDoesNotMatch_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("^Retry [0-9]+").AsRegex();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "^Retry [0-9]+" as regex at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}")
					             ]
					             """)
					.Because("the pattern is matched against the template, not the message");
			}

			[Test]
			public async Task AsRegex_WhenTemplateMatches_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("^Retry .+ of .+$").AsRegex();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task AsSuffix_WhenTemplateDoesNotEndWithExpected_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetryForOrder(new FakeLogger(subject), 1, 5, "ORD-1", null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("of {MaxAttempts}").AsSuffix();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "of {MaxAttempts}" as suffix at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5 for order ORD-1" (message template "Retry {Attempt} of {MaxAttempts} for order {OrderId}")
					             ]
					             """);
			}

			[Test]
			public async Task AsSuffix_WhenTemplateEndsWithExpected_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogRetryForOrder(new FakeLogger(subject), 1, 5, "ORD-1", null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("for order {OrderId}").AsSuffix();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task AsWildcard_WhenTemplateDoesNotMatch_ShouldShowTheFullTemplate()
			{
				FakeLogCollector subject = new();
				LogRetryForOrder(new FakeLogger(subject), 1, 5, "ORD-1", null);

				async Task Act()
					=> await That(subject).Logged()
						.WithMessageTemplate("Retry {Attempt} of {MaxAttempts} for customer *").AsWildcard();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "Retry {Attempt} of {MaxAttempts} for customer *" as wildcard at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5 for order ORD-1" (message template "Retry {Attempt} of {MaxAttempts} for order {OrderId}")
					             ]
					             """)
					.Because("templates often only differ in their last part, so a long template is not shortened");
			}

			[Test]
			public async Task AsWildcard_WhenTemplatesMatch_ShouldCountAllOfThem()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				LogRetryForOrder(logger, 2, 5, "ORD-1", null);
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("Retry {Attempt} of {MaxAttempts}*").AsWildcard()
						.Twice();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task IgnoringCase_WhenTemplateDiffersInCase_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("retry {attempt} of {maxattempts}").IgnoringCase();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task IgnoringCase_WhenTemplateDiffersInMoreThanCase_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("retry {attempt}").IgnoringCase();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "retry {attempt}" ignoring case at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}")
					             ]
					             """);
			}

			[Test]
			public async Task WhenCombinedWithProperty_ShouldListBothDetails()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 2, 5, null);

				async Task Act()
					=> await That(subject).Logged()
						.WithMessageTemplate("Retry {Attempt} of {MaxAttempts}")
						.WithProperty("Attempt").WithValue("3");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "Retry {Attempt} of {MaxAttempts}" and property Attempt with value equal to "3" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 2 of 5" (message template "Retry {Attempt} of {MaxAttempts}", property Attempt "2")
					             ]
					             """);
			}

			[Test]
			public async Task WhenStateIsNotStructured_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				CustomState state = new();
				EventId eventId = new(7);
				logger.Log(LogLevel.Information, eventId, state, null, static (_, _) => "Custom state");

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("Custom state");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "Custom state" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Custom state" (no structured state)
					             ]
					             """)
					.Because("a custom state object has no message template");
			}

			[Test]
			public async Task WhenTemplateDiffersInCase_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 1, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("retry {attempt} of {maxattempts}");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "retry {attempt} of {maxattempts}" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}")
					             ]
					             """)
					.Because("the case is only ignored with IgnoringCase()");
			}

			[Test]
			public async Task WhenTemplateDiffers_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged().WithMessageTemplate("Retry {Attempt}");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with message template "Retry {Attempt}" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 1 of 5" (message template "Retry {Attempt} of {MaxAttempts}"),
					               [Warning] "Retry limit reached" (message template "Retry limit reached")
					             ]
					             """)
					.Because("the template is compared exactly by default");
			}

			[Test]
			public async Task WhenTemplateMatches_ShouldIgnoreTheValues()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 1, 5, null);
				LogRetry(logger, 2, 5, null);
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.WithMessageTemplate("Retry {Attempt} of {MaxAttempts}")
						.Twice();

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, int, int, Exception?> LogRetry =
				LoggerMessage.Define<int, int>(LogLevel.Warning, new EventId(4), "Retry {Attempt} of {MaxAttempts}");

			private static readonly Action<ILogger, int, int, string, Exception?> LogRetryForOrder =
				LoggerMessage.Define<int, int, string>(LogLevel.Warning, new EventId(5),
					"Retry {Attempt} of {MaxAttempts} for order {OrderId}");

			private sealed class CustomState;
		}
	}
}
