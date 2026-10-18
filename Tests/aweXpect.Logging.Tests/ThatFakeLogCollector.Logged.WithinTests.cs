using System.Diagnostics;
using System.Threading;
using aweXpect.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TUnit.Core.Exceptions;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class WithinTests
		{
			[Test]
			public async Task CheckEvery_AfterQuantifierOrThen_ShouldBeAvailable()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				LogWarning(logger, "Retry 1", null);

				async Task Act()
				{
					await That(subject).Logged(LogLevel.Warning).Once()
						.Within(TimeSpan.FromMilliseconds(100)).CheckEvery(TimeSpan.FromMilliseconds(20));
					await That(subject).Logged(LogLevel.Information).Then(r => r.Logged(LogLevel.Warning))
						.Within(TimeSpan.FromSeconds(30)).CheckEvery(TimeSpan.FromMilliseconds(20));
					await That(subject).DidNotLog(LogLevel.Error)
						.Within(TimeSpan.FromMilliseconds(100)).CheckEvery(TimeSpan.FromMilliseconds(20));
				}

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task CheckEvery_ShouldKeepTheFiltersAndQuantifiersAvailable()
			{
				FakeLogCollector subject = new();
				LogWarning(new FakeLogger(subject), "Retry 1", null);

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.Within(TimeSpan.FromMilliseconds(100)).CheckEvery(TimeSpan.FromMilliseconds(20))
						.WithMessage("Retry *").AsWildcard()
						.Once();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task CheckEvery_ShouldReadTheRecordsInTheInterval()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.Within(TimeSpan.FromSeconds(30)).CheckEvery(TimeSpan.FromMilliseconds(500));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(450))
					.Because("the record logged after 50 milliseconds is only read by the second check after the interval");
				await logging;
			}

			[Test]
			public async Task CheckEvery_WhenIntervalIsNotPositive_ShouldThrowArgumentOutOfRangeException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.Within(TimeSpan.FromSeconds(1)).CheckEvery(TimeSpan.Zero);

				await That(Act).Throws<ArgumentOutOfRangeException>()
					.WithParamName("interval");
			}

			[Test]
			public async Task Never_WhenNothingIsLogged_ShouldWaitForTheFullTimeout()
			{
				FakeLogCollector subject = new();
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Never().Within(TimeSpan.FromMilliseconds(200));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150))
					.Because("a record could still be logged until the timeout");
			}

			[Test]
			public async Task Never_WhenRecordIsLoggedDuringTheTimeout_ShouldFailBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				Task logging = LogLater(logger, "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Never().Within(TimeSpan.FromSeconds(30));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning within 0:30,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Retry 1"
					             ]

					             Records:
					             [
					               [Information] "Starting",
					               [Warning] "Retry 1"
					             ]
					             """);
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20))
					.Because("the expectation is violated as soon as the record is logged");
				await logging;
			}

			[Test]
			public async Task Once_WhenRecordIsLoggedDuringTheTimeout_ShouldSucceedAfterTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Once().Within(TimeSpan.FromMilliseconds(500));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(450))
					.Because("a second record could still be logged until the timeout");
				await logging;
			}

			[Test]
			public async Task WhenCanceled_ShouldNotBeDecided()
			{
				FakeLogCollector subject = new();
				using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(100));

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30))
						.WithCancellation(cancellation.Token);

				await That(Act).Throws<InconclusiveTestException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once within 0:30,
					             but it could not be verified, because the evaluation was already canceled
					             """)
					.Because("the records logged until the cancellation do not decide the outcome");
			}

			[Test]
			public async Task WhenNegated_AndRecordIsLoggedDuringTheTimeout_ShouldFailBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.Logged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30)));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning within 0:30,
					             but it logged 1 such record
					             *
					             """).AsWildcard();
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20));
				await logging;
			}

			[Test]
			public async Task WhenNothingIsLogged_ShouldFailAfterTheTimeout()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Within(TimeSpan.FromMilliseconds(100));

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once within 0:00.100,
					             but it did not log any record
					             """);
			}

			[Test]
			public async Task WhenRecordIsLoggedDuringTheTimeout_ShouldSucceedBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20))
					.Because("the expectation is met as soon as the record is logged");
				await logging;
			}

			[Test]
			public async Task WhenSpecifiedTwice_ShouldThrowInvalidOperationException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning)
						.Within(TimeSpan.FromSeconds(1))
						.Within(TimeSpan.FromSeconds(2));

				await That(Act).Throws<InvalidOperationException>()
					.WithMessage("Within cannot be specified more than once.");
			}

			[Test]
			public async Task WhenTimeoutIsNegative_ShouldThrowArgumentOutOfRangeException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning).Within(TimeSpan.FromSeconds(-1));

				await That(Act).Throws<ArgumentOutOfRangeException>();
			}

			private static async Task LogLater(FakeLogger logger, string message)
			{
				await Task.Delay(50);
				LogWarning(logger, message, null);
			}

			private static readonly Action<ILogger, string, Exception?> LogWarning =
				LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3), "{Message}");
		}
	}
}
