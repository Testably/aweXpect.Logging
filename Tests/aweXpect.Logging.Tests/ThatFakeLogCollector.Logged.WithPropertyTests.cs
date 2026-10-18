using System.Globalization;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class WithPropertyTests
		{
			[Test]
			public async Task WhenNegated_AndValueMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 3, 5, null);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.Logged().WithProperty("Attempt").WithValue("3"));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with property Attempt with value equal to "3",
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Retry 3 of 5" (property Attempt "3")
					             ]

					             Records:
					             [
					               [Warning] "Retry 3 of 5" (property Attempt "3")
					             ]
					             """);
			}

			[Test]
			public async Task WhenOptionIsSpecified_ShouldIncludeItInTheExpectation()
			{
				FakeLogCollector subject = new();
				LogOrder(new FakeLogger(subject), "INV-42", null);

				async Task Act()
					=> await That(subject).Logged().WithProperty("OrderId").WithValue("ord-*").AsWildcard()
						.IgnoringCase();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with property OrderId with value matching "ord-*" ignoring case at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Order INV-42" (property OrderId "INV-42")
					             ]
					             """);
			}

			[Test]
			public async Task WhenPatternIsInvalid_ShouldThrowArgumentException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged().WithProperty("OrderId").WithValue("(").AsRegex();

				await That(Act).Throws<ArgumentException>()
					.Because("an unusable pattern is rejected even when no record is compared with it");
			}

			[Test]
			public async Task WhenPropertyExists_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 3, 5, null);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).WithProperty("Attempt");

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenPropertyIsMissing_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 3, 5, null);

				async Task Act()
					=> await That(subject).Logged().WithProperty("OrderId");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with property OrderId at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 3 of 5" (no property OrderId)
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
					=> await That(subject).Logged().WithProperty("Attempt");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with property Attempt at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Custom state" (no structured state)
					             ]
					             """)
					.Because("a custom state object has no properties to compare");
			}

			[Test]
			public async Task WhenValueDiffers_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 2, 5, null);
				LogRetry(logger, 3, 4, null);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.WithProperty("Attempt").WithValue("3")
						.WithProperty("MaxAttempts").WithValue("5");

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning and property Attempt with value equal to "3" and property MaxAttempts with value equal to "5" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 2 of 5" (property Attempt "2", property MaxAttempts "5"),
					               [Warning] "Retry 3 of 4" (property Attempt "3", property MaxAttempts "4")
					             ]
					             """)
					.Because("a record must have all properties");
			}

			[Test]
			public async Task WhenValueIsNull_ShouldMatchNull()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogOrder(logger, "ORD-42", null);
				LogOrder(logger, null, null);

				async Task Act()
					=> await That(subject).Logged().WithProperty("OrderId").WithValue(null).Once();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenValueIsNumber_ShouldCompareItInTheInvariantCulture()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				CultureInfo previousCulture = CultureInfo.CurrentCulture;
				CultureInfo.CurrentCulture = new CultureInfo("de-DE");
				try
				{
					LogCharged(logger, 1.5, null);
				}
				finally
				{
					CultureInfo.CurrentCulture = previousCulture;
				}

				async Task Act()
					=> await That(subject).Logged().WithProperty("Amount").WithValue("1.5");

				await That(Act).DoesNotThrow()
					.Because("the FakeLogger stores the values in the invariant culture");
			}

			[Test]
			public async Task WhenValueIsSpecifiedTwice_ShouldThrowInvalidOperationException()
			{
				FakeLogCollector subject = new();

				async Task Act()
				{
					LoggedPropertyResult property = That(subject).Logged().WithProperty("Attempt");
					_ = property.WithValue("1");
					await property.WithValue("2");
				}

				await That(Act).Throws<InvalidOperationException>()
					.WithMessage("WithValue cannot be specified more than once.");
			}

			[Test]
			public async Task WhenValueMatches_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 2, 5, null);
				LogRetry(logger, 3, 5, null);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).WithProperty("Attempt").WithValue("3").Once();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenValueMatchesThePattern_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogOrder(logger, "ORD-42", null);
				LogOrder(logger, "INV-42", null);

				async Task Act()
					=> await That(subject).Logged().WithProperty("OrderId").WithValue("ORD-*").AsWildcard().Once();

				await That(Act).DoesNotThrow();
			}

			private static readonly Action<ILogger, double, Exception?> LogCharged =
				LoggerMessage.Define<double>(LogLevel.Information, new EventId(5), "Charged {Amount}");

			private static readonly Action<ILogger, string?, Exception?> LogOrder =
				LoggerMessage.Define<string?>(LogLevel.Information, new EventId(6), "Order {OrderId}");

			private static readonly Action<ILogger, int, int, Exception?> LogRetry =
				LoggerMessage.Define<int, int>(LogLevel.Warning, new EventId(4), "Retry {Attempt} of {MaxAttempts}");

			private sealed class CustomState;
		}
	}
}
