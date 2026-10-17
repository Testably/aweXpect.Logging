using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithEventIdTests
		{
			[Fact]
			public async Task WhenCombinedWithOtherFilters_ShouldListAllComparedDetails()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Orders");
				TimeoutException exception = new();
				LogRetryLimitReached(logger, exception);

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning)
						.WithCategory("MyApp.Orders")
						.WithEventId(1001)
						.WithException<TimeoutException>();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and exception of type TimeoutException and category "MyApp.Orders" and event ID 1001 at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry limit reached" (TimeoutException, category "MyApp.Orders", event ID 1002)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenEventIdDiffers_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogOrderPlaced(logger, null);
				LogRetryLimitReached(logger, null);

				async Task Act()
					=> await That(subject).HasLogged().WithEventId(1003);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with event ID 1003 at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Order placed" (event ID 1001),
					               [Warning] "Retry limit reached" (event ID 1002)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenEventIdMatches_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				LogOrderPlaced(logger, null);
				LogRetryLimitReached(logger, null);

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithEventId(1002).Once();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNegated_AndEventIdMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				LogOrderPlaced(new FakeLogger(subject), null);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged().WithEventId(1001));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with event ID 1001,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Information] "Order placed" (event ID 1001)
					             ]

					             Records:
					             [
					               [Information] "Order placed" (event ID 1001)
					             ]
					             """);
			}

			private static readonly Action<ILogger, Exception?> LogOrderPlaced =
				LoggerMessage.Define(LogLevel.Information, new EventId(1001), "Order placed");

			private static readonly Action<ILogger, Exception?> LogRetryLimitReached =
				LoggerMessage.Define(LogLevel.Warning, new EventId(1002), "Retry limit reached");
		}
	}
}
