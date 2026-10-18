using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class DidNotLog
	{
		public sealed class Tests
		{
			[Test]
			public async Task OrHigher_WhenHigherLevelWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).OrHigher();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning or higher,
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

			[Test]
			public async Task OrHigher_WhenOnlyLowerLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogDebug("Connecting");
				logger.LogInformation("Connected");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).OrHigher();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning);

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning,
					             but it was <null>
					             """);
			}

			[Test]
			public async Task WhenCombinedWithLogged_ShouldVerifyBoth()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information).And.DidNotLog(LogLevel.Error);

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information at least once and did not log a record with level Error,
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

			[Test]
			public async Task WhenItemsArePlural_ShouldUsePluralVerb()
			{
				FakeLogCollector[] subject = [new(), new(),];
				new FakeLogger(subject[0]).LogWarning("Retry limit reached");

				async Task Act()
					=> await That(new { Collectors = subject, })
						.Whose(x => x.Collectors, c => c.All().ComplyWith(item => item.DidNotLog(LogLevel.Warning)));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that new { Collectors = subject, }
					             whose Collectors did not log a record with level Warning for all items,
					             *
					             """).AsWildcard()
					.Because("the items are the plural subject of the expectation");
			}

			[Test]
			public async Task WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning);

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning);

				await That(Act).DoesNotThrow()
					.Because("the level must match exactly, so a more severe record does not count");
			}

			[Test]
			public async Task WhenRecordsWithLevelWereLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning);

				await That(Act).Throws<FailException>()
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

			[Test]
			public async Task WithCategory_WhenMatchingCategoryWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Orders.OrderService");
				logger.LogError("Order failed");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Error).WithCategory("MyApp.Orders.").AsPrefix();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Error and category "MyApp.Orders." as prefix,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Order failed" (category "MyApp.Orders.OrderService")
					             ]

					             Records:
					             [
					               [Error] "Order failed" (category "MyApp.Orders.OrderService")
					             ]
					             """);
			}

			[Test]
			public async Task WithCategory_WhenOnlyOtherCategoriesLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Payments");
				logger.LogError("Payment declined");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Error).WithCategory("MyApp.Orders");

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithEventId_WhenEventIdWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentDeclined(new FakeLogger(subject), null);

				async Task Act()
					=> await That(subject).DidNotLog().WithEventId(2001);

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with event ID 2001,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Payment declined" (event ID 2001)
					             ]

					             Records:
					             [
					               [Error] "Payment declined" (event ID 2001)
					             ]
					             """);
			}

			[Test]
			public async Task WithException_WhenExceptionIsOfOtherType_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new InvalidOperationException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Error).WithException<TimeoutException>();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithException_WhenExceptionIsOfType_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				TimeoutException exception = new();
				logger.LogError(new InvalidOperationException(), "Retry 1");
				logger.LogCritical(exception, "Retry 2");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Error).OrHigher().WithException<TimeoutException>();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Error or higher and exception of type TimeoutException,
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

			[Test]
			public async Task WithMessageTemplate_AsPrefix_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("Payment retried").AsPrefix();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "Payment retried" as prefix,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessageTemplate_AsRegex_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("^Payment .+ times$").AsRegex();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "^Payment .+ times$" as regex,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessageTemplate_AsSuffix_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("{Attempt} times").AsSuffix();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "{Attempt} times" as suffix,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessageTemplate_AsWildcard_WhenOnlyOtherTemplatesWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("Order * times").AsWildcard();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithMessageTemplate_AsWildcard_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("Payment * times").AsWildcard();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "Payment * times" as wildcard,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessageTemplate_IgnoringCase_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("payment retried {attempt} times")
						.IgnoringCase();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "payment retried {attempt} times" ignoring case,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessageTemplate_WhenTemplateDiffersInCase_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("payment retried {attempt} times");

				await That(Act).DoesNotThrow()
					.Because("the template is compared exactly by default");
			}

			[Test]
			public async Task WithMessageTemplate_WhenTemplateWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithMessageTemplate("Payment retried {Attempt} times");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with message template "Payment retried {Attempt} times",
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (message template "Payment retried {Attempt} times")
					             ]
					             """);
			}

			[Test]
			public async Task WithMessage_WhenMessageDiffers_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Connection lost");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).WithMessage("Retry *").AsWildcard();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithMessage_WhenMessageMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Connection lost");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).WithMessage("retry *").AsWildcard()
						.IgnoringCase();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning and message matching "retry *" ignoring case,
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

			[Test]
			public async Task WithProperty_WhenOnlyOtherValuesWereLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).WithProperty("Attempt").WithValue("5");

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithProperty_WhenPropertyWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogPaymentRetried(new FakeLogger(subject), 3, null);

				async Task Act()
					=> await That(subject).DidNotLog().WithProperty("Attempt");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with property Attempt,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Payment retried 3 times" (property Attempt "3")
					             ]

					             Records:
					             [
					               [Warning] "Payment retried 3 times" (property Attempt "3")
					             ]
					             """);
			}

			[Test]
			public async Task WithoutLevel_WhenAnyRecordWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogTrace("Polling");

				async Task Act()
					=> await That(subject).DidNotLog();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record,
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

			[Test]
			public async Task WithoutLevel_WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).DidNotLog();

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, Exception?> LogPaymentDeclined =
				LoggerMessage.Define(LogLevel.Error, new EventId(2001), "Payment declined");

			private static readonly Action<ILogger, int, Exception?> LogPaymentRetried =
				LoggerMessage.Define<int>(LogLevel.Warning, new EventId(2002), "Payment retried {Attempt} times");
		}

		public sealed class NegatedTests
		{
			[Test]
			public async Task WhenCollectorIsNull_ShouldFail()
			{
				FakeLogCollector? subject = null;

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DidNotLog(LogLevel.Warning));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it was <null>
					             """)
					.Because("a null collector cannot tell whether a record was logged");
			}

			[Test]
			public async Task WhenNothingWasLogged_ShouldFail()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DidNotLog(LogLevel.Warning));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it did not log any record
					             """);
			}

			[Test]
			public async Task WhenRecordWithLevelWasLogged_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DidNotLog(LogLevel.Warning));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
