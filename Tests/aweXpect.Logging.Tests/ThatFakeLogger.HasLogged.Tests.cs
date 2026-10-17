using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogger
{
	public sealed class HasLogged
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenCombinedWithAnd_ShouldVerifyTheCollectorOfTheLogger()
			{
				FakeLogger subject = new();
				subject.LogInformation("Starting");
				subject.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).And.HasNotLogged(LogLevel.Error);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Information at least once and has not logged a record with level Error,
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

			[Fact]
			public async Task WhenLoggerIsGeneric_ShouldVerifyItsRecords()
			{
				FakeLogger<OrderService> subject = new();
				subject.LogInformation("Placing order");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).WithCategory<OrderService>()
						.WithMessage("Placing *").AsWildcard().Once();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenLoggerIsNull_ShouldFail()
			{
				FakeLogger? subject = null;

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenNegated_AndRecordWasLogged_ShouldFail()
			{
				FakeLogger subject = new();
				subject.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record,
					             but it logged 1 such record
					             *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenNothingWasLogged_ShouldFail()
			{
				FakeLogger subject = new();

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning at least once,
					             but it did not log any record
					             """);
			}

			[Fact]
			public async Task WhenOtherLoggerWritesToTheSameCollector_ShouldConsiderItsRecords()
			{
				FakeLogCollector collector = new();
				FakeLogger subject = new(collector);
				FakeLogger otherLogger = new(collector);
				otherLogger.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning);

				await That(Act).DoesNotThrow()
					.Because("the expectation verifies the records of the collector of the logger");
			}

			[Fact]
			public async Task WhenRecordsAreLoggedInOrder_ShouldSucceed()
			{
				FakeLogger subject = new();
				subject.LogInformation("Starting");
				subject.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information)
						.Then(r => r.HasLogged(LogLevel.Warning))
						.Within(TimeSpan.FromSeconds(1));

				await That(Act).DoesNotThrow();
			}

			public sealed class OrderService;
		}
	}

	public sealed class HasNotLogged
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogger subject = new();

				async Task Act()
					=> await That(subject).HasNotLogged();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenRecordWasLogged_ShouldFail()
			{
				FakeLogger subject = new();
				subject.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).HasNotLogged(LogLevel.Warning).OrHigher();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with level Warning or higher,
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
			}
		}
	}
}
