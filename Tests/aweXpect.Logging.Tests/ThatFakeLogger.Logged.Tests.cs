using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogger
{
	public sealed class Logged
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
					=> await That(subject).Logged(LogLevel.Information).And.DidNotLog(LogLevel.Error);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Information at least once and did not log a record with level Error,
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

			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenLevelMethodIsUsed_ShouldExpectThatLevel(LogLevel level)
			{
				FakeLogger subject = new();
				foreach (LogLevel otherLevel in LogLevels.Except(level))
				{
					subject.Log(otherLevel, "Other record");
				}

				async Task Act()
					=> await (level switch
					{
						LogLevel.Trace => That(subject).LoggedTrace(),
						LogLevel.Debug => That(subject).LoggedDebug(),
						LogLevel.Information => That(subject).LoggedInformation(),
						LogLevel.Warning => That(subject).LoggedWarning(),
						LogLevel.Error => That(subject).LoggedError(),
						LogLevel.Critical => That(subject).LoggedCritical(),
						_ => throw new ArgumentOutOfRangeException(nameof(level)),
					});

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              logged a record with level {level} at least once,
					              but it did not log any such record
					              *
					              """).AsWildcard();
			}

			[Fact]
			public async Task WhenLoggerIsGeneric_ShouldVerifyItsRecords()
			{
				FakeLogger<OrderService> subject = new();
				subject.LogInformation("Placing order");

				async Task Act()
					=> await That(subject).Logged(LogLevel.Information).WithCategory<OrderService>()
						.WithMessage("Placing *").AsWildcard().Once();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenLoggerIsNull_ShouldFail()
			{
				FakeLogger? subject = null;

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenNegated_AndRecordWasLogged_ShouldFail()
			{
				FakeLogger subject = new();
				subject.LogWarning("Retry 1");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.Logged());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record,
					             but it logged 1 such record
					             *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenNothingWasLogged_ShouldFail()
			{
				FakeLogger subject = new();

				async Task Act()
					=> await That(subject).Logged(LogLevel.Warning);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             logged a record with level Warning at least once,
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
					=> await That(subject).Logged(LogLevel.Warning);

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
					=> await That(subject).Logged(LogLevel.Information)
						.Then(r => r.Logged(LogLevel.Warning))
						.Within(TimeSpan.FromSeconds(1));

				await That(Act).DoesNotThrow();
			}

			public sealed class OrderService;
		}
	}

	public sealed class DidNotLog
	{
		public sealed class Tests
		{
			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenLevelMethodIsUsed_ShouldExpectThatLevel(LogLevel level)
			{
				FakeLogger subject = new();
				subject.Log(level, "Retry limit reached");

				async Task Act()
					=> await (level switch
					{
						LogLevel.Trace => That(subject).DidNotLogTrace(),
						LogLevel.Debug => That(subject).DidNotLogDebug(),
						LogLevel.Information => That(subject).DidNotLogInformation(),
						LogLevel.Warning => That(subject).DidNotLogWarning(),
						LogLevel.Error => That(subject).DidNotLogError(),
						LogLevel.Critical => That(subject).DidNotLogCritical(),
						_ => throw new ArgumentOutOfRangeException(nameof(level)),
					});

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              did not log a record with level {level},
					              but it logged 1 such record
					              *
					              """).AsWildcard();
			}
			[Fact]
			public async Task WhenNothingWasLogged_ShouldSucceed()
			{
				FakeLogger subject = new();

				async Task Act()
					=> await That(subject).DidNotLog();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenRecordWasLogged_ShouldFail()
			{
				FakeLogger subject = new();
				subject.LogError("Retry limit reached");

				async Task Act()
					=> await That(subject).DidNotLog(LogLevel.Warning).OrHigher();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             did not log a record with level Warning or higher,
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
