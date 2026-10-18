using System.Diagnostics;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class ThenTests
		{
			[Test]
			public async Task WhenCombinedWithQuantifier_ShouldThrowInvalidOperationException()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogInformation("Starting");

				async Task Act()
				{
					LoggedResult started = That(subject).Logged(LogLevel.Information);
					_ = started.Twice();
					await started.Then(r => r.Logged(LogLevel.Warning));
				}

				await That(Act).Throws<InvalidOperationException>()
					.WithMessage("Then cannot be combined with a quantifier.")
					.Because("the order implies that the first record was logged at least once");
			}

			[Test]
			public async Task WhenFirstRecordIsMissing_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information, then
					             a record with level Warning in order,
					             but it did not log a record with level Information at all

					             Records:
					             [
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Test]
			public async Task WhenNegated_AndRecordsAreLoggedInOrder_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning)));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information, then
					             a record with level Warning not in order,
					             but it did

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Test]
			public async Task WhenNegated_AndRecordsAreNotLoggedInOrder_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning)));

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenOneRecordMatchesTwoSteps_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Then(r => r.Logged(LogLevel.Warning));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning, then
					             a record with level Warning in order,
					             but it logged a record with level Warning too early

					             Records:
					             [
					               [Warning] "Retry 1"
					             ]
					             """)
					.Because("each step needs its own record after the one of the previous step");
			}

			[Test]
			public async Task WhenOtherRecordsAreLoggedInBetween_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogInformation("Connecting");
				logger.LogWarning("Retry 1");
				logger.LogInformation("Connecting");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(
							r => r.Logged(LogLevel.Warning),
							r => r.Logged(LogLevel.Error));

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenRecordIsLoggedDuringTheTimeout_ShouldSucceedBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				Task logging = LogLater(logger);
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning))
						.Within(TimeSpan.FromSeconds(30));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20))
					.Because("further records cannot undo the order, so it stops as soon as it is complete");
				await logging;
			}

			[Test]
			public async Task WhenRecordsAreLoggedInOrder_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");
				logger.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(
							r => r.Logged(LogLevel.Warning),
							r => r.Logged(LogLevel.Error));

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenStepIsLoggedTooEarly_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogWarning("Retry 1");
				logger.LogInformation("Starting");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information, then
					             a record with level Warning in order,
					             but it logged a record with level Warning too early

					             Records:
					             [
					               [Warning] "Retry 1",
					               [Information] "Starting"
					             ]
					             """);
			}

			[Test]
			public async Task WhenStepIsNotLoggedAtAll_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information)
						.Then(
							r => r.Logged(LogLevel.Warning),
							r => r.Logged(LogLevel.Error));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information, then
					             a record with level Warning, then
					             a record with level Error in order,
					             but it did not log a record with level Error at all

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1"
					             ]
					             """);
			}

			[Test]
			public async Task WhenStepsAreNull_ShouldThrowArgumentNullException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged().Then(null!);

				await That(Act).Throws<ArgumentNullException>()
					.WithParamName("steps");
			}

			[Test]
			public async Task WhenStepsHaveFilters_ShouldDescribeAndListThem()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				TimeoutException exception = new();
				logger.LogInformation("Starting");
				LogRetry(logger, 1, null);
				logger.LogCritical(exception, "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information).WithMessage("Starting")
						.Then(
							r => r.Logged(LogLevel.Warning).WithProperty("Attempt").WithValue("2"),
							r => r.Logged(LogLevel.Error).OrHigher().WithException<TimeoutException>());

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information and message equal to "Starting", then
					             a record with level Warning and property Attempt with value equal to "2", then
					             a record with level Error or higher and exception of type TimeoutException in order,
					             but it did not log a record with level Warning and property Attempt with value equal to "2" at all

					             Records:
					             [
					               [Information] "Starting" (no property Attempt),
					               [Warning] "Retry 1" (property Attempt "1"),
					               [Critical] "Retry limit reached" (TimeoutException, no property Attempt)
					             ]
					             """);
			}

			private static async Task LogLater(FakeLogger logger)
			{
				await Task.Delay(50);
				LogRetry(logger, 1, null);
			}

			private static readonly Action<ILogger, int, Exception?> LogRetry =
				LoggerMessage.Define<int>(LogLevel.Warning, new EventId(8), "Retry {Attempt}");
		}
	}
}
