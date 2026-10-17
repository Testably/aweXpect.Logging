using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithPropertyTests
		{
			[Fact]
			public async Task WhenNegated_AndPropertyMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 3, 5, null);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged().WithProperty("Attempt", "3"));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with property Attempt equal to "3",
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

			[Fact]
			public async Task WhenPropertyHasOtherValue_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 2, 5, null);
				LogRetry(logger, 3, 4, null);

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning)
						.WithProperty("Attempt", "3")
						.WithProperty("MaxAttempts", "5");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and property Attempt equal to "3" and property MaxAttempts equal to "5" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 2 of 5" (property Attempt "2", property MaxAttempts "5"),
					               [Warning] "Retry 3 of 4" (property Attempt "3", property MaxAttempts "4")
					             ]
					             """)
					.Because("a record must have all properties");
			}

			[Fact]
			public async Task WhenPropertyHasValue_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogRetry(logger, 2, 5, null);
				LogRetry(logger, 3, 5, null);

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithProperty("Attempt", "3").Once();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenPropertyIsMissing_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogRetry(new FakeLogger(subject), 3, 5, null);

				async Task Act()
					=> await That(subject).HasLogged().WithProperty("OrderId", "42");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with property OrderId equal to "42" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry 3 of 5" (no property OrderId)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenPropertyValueIsNull_ShouldMatchNull()
			{
				FakeLogCollector subject = new();
				LogOrder(new FakeLogger(subject), null, null);

				async Task Act()
					=> await That(subject).HasLogged().WithProperty("OrderId", null);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenPropertyValueIsNumber_ShouldCompareItInTheInvariantCulture()
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
					=> await That(subject).HasLogged().WithProperty("Amount", "1.5");

				await That(Act).DoesNotThrow()
					.Because("the FakeLogger stores the values in the invariant culture");
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
					=> await That(subject).HasLogged().WithProperty("Attempt", "3");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with property Attempt equal to "3" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Custom state" (no structured state)
					             ]
					             """)
					.Because("a custom state object has no properties to compare");
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
