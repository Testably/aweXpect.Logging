using aweXpect.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class RecordsContextTests
		{
			[Fact]
			public async Task WhenManyRecordsMatch_ShouldListOnlyTheFirstMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				for (int i = 0; i < 12; i++)
				{
					logger.LogWarning("Retry");
				}

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Never();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning,
					             but it logged 12 such records

					             Matching records:
					             [
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               (… and 2 more)
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               [Warning] "Retry",
					               (… and 3 more)
					             ]
					             """);
			}

			[Fact]
			public async Task WhenNegatedQuantifierHasLowerBoundAboveOne_ShouldListNotMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Warning).AtMost(1.Times()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning more than once,
					             but it logged 1 such record

					             Not matching records:
					             [
					               [Information] "Starting"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1"
					             ]
					             """)
					.Because("the negation of an upper bound is a lower bound");
			}

			[Fact]
			public async Task WhenNegatedQuantifierHasNoComplement_ShouldOnlyListTheRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject)
						.DoesNotComplyWith(it => it.Logged(LogLevel.Warning).Between(1).And(2.Times()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning between 1 and 2 times,
					             but it logged 1 such record

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenQuantifierHasBothBounds_ShouldOnlyListTheRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Once();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning exactly once,
					             but it logged 2 such records

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Information] "Starting",
					               [Warning] "Retry 2"
					             ]
					             """)
					.Because("an exact amount can fail in both directions, so neither matching nor not matching records explain the failure");
			}

			[Fact]
			public async Task WhenQuantifierHasLowerBoundAboveOne_AndAllRecordsMatch_ShouldOnlyListTheRecords()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).AtLeast(2.Times());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least twice,
					             but it logged 1 such record

					             Records:
					             [
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenQuantifierHasLowerBoundAboveOne_ShouldListNotMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).AtLeast(2.Times());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least twice,
					             but it logged 1 such record

					             Not matching records:
					             [
					               [Information] "Starting",
					               [Error] "Retry limit reached"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1",
					               [Error] "Retry limit reached"
					             ]
					             """);
			}

			[Fact]
			public async Task WhenQuantifierHasUpperBound_ShouldListMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 2");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).AtMost(1.Times());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at most once,
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
		}
	}
}
