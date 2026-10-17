using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class DidNotLog
	{
		public sealed class WithinTests
		{
			[Fact]
			public async Task WhenNothingIsLogged_ShouldSucceedAfterTheTimeout()
			{
				FakeLogCollector subject = new();
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).OrHigher()
						.Within(TimeSpan.FromMilliseconds(200));

				await That(Act).DoesNotThrow();
				await That(stopwatch.Elapsed).IsGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150))
					.Because("a record could still be logged until the timeout");
			}

			[Fact]
			public async Task WhenRecordIsLoggedDuringTheTimeout_ShouldFailBeforeTheTimeout()
			{
				FakeLogCollector subject = new();
				Task logging = LogLater(new FakeLogger(subject), "Retry limit reached");
				Stopwatch stopwatch = Stopwatch.StartNew();

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).OrHigher()
						.WithMessage("Retry *").AsWildcard()
						.Within(TimeSpan.FromSeconds(30));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning or higher and message matching "Retry *" within 0:30,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Retry limit reached"
					             ]

					             Records:
					             [
					               [Error] "Retry limit reached"
					             ]
					             """);
				await That(stopwatch.Elapsed).IsLessThan(TimeSpan.FromSeconds(20))
					.Because("the expectation is violated as soon as the record is logged");
				await logging;
			}

			private static async Task LogLater(FakeLogger logger, string message)
			{
				await Task.Delay(50);
				LogError(logger, message, null);
			}

			private static readonly Action<ILogger, string, Exception?> LogError =
				LoggerMessage.Define<string>(LogLevel.Error, new EventId(4), "{Message}");
		}
	}
}
