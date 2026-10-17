using aweXpect.Core;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class Logged
	{
		public sealed class LevelTests
		{
			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldFail(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				foreach (LogLevel otherLevel in LogLevels.Except(level))
				{
					logger.Log(otherLevel, "Other record");
				}

				async Task Act()
					=> await LoggedWithLevel(That(subject), level);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              logged a record with level {level} at least once,
					              but it did not log any such record
					              *
					              """).AsWildcard();
			}

			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenRecordWithLevelWasLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).Log(level, "Retry limit reached");

				async Task Act()
					=> await LoggedWithLevel(That(subject), level).WithMessage("Retry limit reached").Once();

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenStepOnlyMatchesOtherLevels_ShouldFail(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.Log(level, "Starting");
				foreach (LogLevel otherLevel in LogLevels.Except(level))
				{
					logger.Log(otherLevel, "Other record");
				}

				async Task Act()
					=> await That(subject).Logged().WithMessage("Starting")
						.Then(r => StepWithLevel(r, level));

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              *
					              a record with level {level} in order,
					              *
					              """).AsWildcard();
			}

			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenStepRecordWithLevelWasLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				logger.LogInformation("Starting");
				logger.Log(level, "Retry limit reached");

				async Task Act()
					=> await That(subject).Logged().WithMessage("Starting")
						.Then(r => StepWithLevel(r, level).WithMessage("Retry limit reached"));

				await That(Act).DoesNotThrow();
			}

			private static LoggedLevelResult LoggedWithLevel(IThat<FakeLogCollector?> that, LogLevel level)
				=> level switch
				{
					LogLevel.Trace => that.LoggedTrace(),
					LogLevel.Debug => that.LoggedDebug(),
					LogLevel.Information => that.LoggedInformation(),
					LogLevel.Warning => that.LoggedWarning(),
					LogLevel.Error => that.LoggedError(),
					LogLevel.Critical => that.LoggedCritical(),
					_ => throw new ArgumentOutOfRangeException(nameof(level)),
				};

			private static LogRecordStepLevelResult StepWithLevel(LogRecordStep step, LogLevel level)
				=> level switch
				{
					LogLevel.Trace => step.LoggedTrace(),
					LogLevel.Debug => step.LoggedDebug(),
					LogLevel.Information => step.LoggedInformation(),
					LogLevel.Warning => step.LoggedWarning(),
					LogLevel.Error => step.LoggedError(),
					LogLevel.Critical => step.LoggedCritical(),
					_ => throw new ArgumentOutOfRangeException(nameof(level)),
				};
		}
	}
}
