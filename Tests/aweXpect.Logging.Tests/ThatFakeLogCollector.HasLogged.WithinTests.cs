using System.Diagnostics;
using System.Threading;
using aweXpect.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithinTests
		{
			[Fact]
			public async Task Never_WhenNothingIsLogged_ShouldWaitForTheFullTimeout()
			{
				FakeLogCollector subject = new();
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Never().Within(TimeSpan.FromMilliseconds(200));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150))
					.Because("a record could still be logged until the timeout");
			}

			[Fact]
			public async Task Never_WhenRecordIsLoggedDuringTheTimeout_ShouldFailBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				Task logging = LogLater(logger, "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Never().Within(TimeSpan.FromSeconds(30));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning within 0:30,
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

			[Fact]
			public async Task Once_WhenRecordIsLoggedDuringTheTimeout_ShouldSucceedAfterTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Once().Within(TimeSpan.FromMilliseconds(500));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(450))
					.Because("a second record could still be logged until the timeout");
				await logging;
			}

			[Fact]
			public async Task WhenCanceled_ShouldNotBeDecided()
			{
				FakeLogCollector subject = new();
				using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(100));

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30))
						.WithCancellation(cancellation.Token);

				await That(Act).Throws<InconclusiveException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once within 0:30,
					             but it could not be verified, because the evaluation was already canceled
					             """)
					.Because("the records logged until the cancellation do not decide the outcome");
			}

			[Fact]
			public async Task WhenNegated_AndRecordIsLoggedDuringTheTimeout_ShouldFailBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.HasLogged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning within 0:30,
					             but it logged 1 such record
					             *
					             """).AsWildcard();
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20));
				await logging;
			}

			[Fact]
			public async Task WhenNothingIsLogged_ShouldFailAfterTheTimeout()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Within(TimeSpan.FromMilliseconds(100));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once within 0:00.100,
					             but it did not log any record
					             """);
			}

			[Fact]
			public async Task WhenRecordIsLoggedDuringTheTimeout_ShouldSucceedBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry 1");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20))
					.Because("the expectation is met as soon as the record is logged");
				await logging;
			}

			[Fact]
			public async Task WhenSpecifiedTwice_ShouldThrowInvalidOperationException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning)
						.Within(TimeSpan.FromSeconds(1))
						.Within(TimeSpan.FromSeconds(2));

				await That(Act).Throws<InvalidOperationException>()
					.WithMessage("Within cannot be specified more than once.");
			}

			[Fact]
			public async Task WhenTimeoutIsNegative_ShouldThrowArgumentOutOfRangeException()
			{
				FakeLogCollector subject = new();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).Within(TimeSpan.FromSeconds(-1));

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
