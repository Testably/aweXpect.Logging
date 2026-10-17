using aweXpect.Core;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class DidNotLog
	{
		public sealed class LevelTests
		{
			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenOnlyOtherLevelsWereLogged_ShouldSucceed(LogLevel level)
			{
				FakeLogCollector subject = new();
				FakeLogger logger = new(subject);
				foreach (LogLevel otherLevel in LogLevels.Except(level))
				{
					logger.Log(otherLevel, "Other record");
				}

				async Task Act()
					=> await DidNotLogWithLevel(That(subject), level);

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[MemberData(nameof(LogLevels.Data), MemberType = typeof(LogLevels))]
			public async Task WhenRecordWithLevelWasLogged_ShouldFail(LogLevel level)
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).Log(level, "Retry limit reached");

				async Task Act()
					=> await DidNotLogWithLevel(That(subject), level).WithMessage("Retry limit reached");

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              did not log a record with level {level} and message equal to "Retry limit reached",
					              but it logged 1 such record
					              *
					              """).AsWildcard();
			}

			private static DidNotLogLevelResult DidNotLogWithLevel(IThat<FakeLogCollector?> that, LogLevel level)
				=> level switch
				{
					LogLevel.Trace => that.DidNotLogTrace(),
					LogLevel.Debug => that.DidNotLogDebug(),
					LogLevel.Information => that.DidNotLogInformation(),
					LogLevel.Warning => that.DidNotLogWarning(),
					LogLevel.Error => that.DidNotLogError(),
					LogLevel.Critical => that.DidNotLogCritical(),
					_ => throw new ArgumentOutOfRangeException(nameof(level)),
				};
		}
	}
}
