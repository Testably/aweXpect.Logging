using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class WithExceptionTests
		{
			[Test]
			public async Task WhenCombinedWithMessageAndQuantifier_ShouldOnlyCountMatchingRecords()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogError(new TimeoutException(), "Retry 1");
				logger.LogError(new InvalidOperationException(), "Retry 2");
				logger.LogError(new TimeoutException(), "Connection lost");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Error).OrHigher()
						.WithMessage("Retry *").AsWildcard()
						.WithException<TimeoutException>()
						.Twice();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Error or higher and message matching "Retry *" and exception of type TimeoutException exactly twice,
					             but it logged 1 such record

					             Records:
					             [
					               [Error] "Retry 1" (TimeoutException),
					               [Error] "Retry 2" (InvalidOperationException),
					               [Error] "Connection lost" (TimeoutException)
					             ]
					             """);
			}

			[Test]
			public async Task WhenExceptionIsDerived_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new FileNotFoundException(), "Configuration missing");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Error).WithException<IOException>();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenExceptionIsOfOtherType_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new InvalidOperationException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Error).WithException<TimeoutException>();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Error and exception of type TimeoutException at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Error] "Retry limit reached" (InvalidOperationException)
					             ]
					             """);
			}

			[Test]
			public async Task WhenExceptionIsOfType_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new TimeoutException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Error).WithException<TimeoutException>();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WhenNegated_AndExceptionMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError(new TimeoutException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.Logged(LogLevel.Error).WithException<TimeoutException>());

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Error and exception of type TimeoutException,
					             but it logged 1 such record

					             Matching records:
					             [
					               [Error] "Retry limit reached" (TimeoutException)
					             ]

					             Records:
					             [
					               [Error] "Retry limit reached" (TimeoutException)
					             ]
					             """);
			}

			[Test]
			public async Task WhenRecordHasNoException_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Error).WithException<Exception>();

				await That(Act).Throws<FailException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Error and exception of type Exception at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Error] "Retry limit reached"
					             ]
					             """);
			}

			[Test]
			public async Task WhenSpecifiedTwice_ShouldRequireBothTypes()
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogError(new IOException(), "Disk full");
				logger.LogError(new FileNotFoundException(), "Configuration missing");
				logger.LogError(new InvalidOperationException(), "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged()
						.WithException<IOException>()
						.WithException<FileNotFoundException>()
						.Once();

				await That(Act).DoesNotThrow();
			}

			[Test]
			public async Task WithoutLevel_ShouldOnlyCompareTheException()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning(new TimeoutException(), "Retry 1");

				async Task Act()
					=> await That(subject).Logged().WithException<TimeoutException>();

				await That(Act).DoesNotThrow();
			}
		}
	}
}
