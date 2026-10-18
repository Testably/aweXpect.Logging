using aweXpect.Core;
using aweXpect.Core.Extending;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect;

/// <summary>
///     Expectations on <see cref="FakeLogger" /> values, which verify the records of its
///     <see cref="FakeLogger.Collector" />.
/// </summary>
/// <remarks>
///     The results continue with <c>And</c> or <c>Or</c> on the collector of the logger, so that further expectations
///     on its records, e.g. <c>DidNotLog</c>, can follow.
/// </remarks>
public static class ThatFakeLogger
{
	/// <inheritdoc cref="ThatFakeLogCollector.Logged(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedResult Logged(this IThat<FakeLogger?> subject)
		=> new CollectorOfLogger(subject).Logged();

	/// <inheritdoc cref="ThatFakeLogCollector.Logged(IThat{FakeLogCollector}, LogLevel)" />
	[GuaranteesNotNull]
	public static LoggedLevelResult Logged(this IThat<FakeLogger?> subject, LogLevel level)
		=> new CollectorOfLogger(subject).Logged(level);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLog(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogResult DidNotLog(this IThat<FakeLogger?> subject)
		=> new CollectorOfLogger(subject).DidNotLog();

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLog(IThat{FakeLogCollector}, LogLevel)" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLog(this IThat<FakeLogger?> subject, LogLevel level)
		=> new CollectorOfLogger(subject).DidNotLog(level);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedTrace(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedTrace(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Trace);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedDebug(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedDebug(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Debug);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedInformation(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedInformation(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Information);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedWarning(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedWarning(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Warning);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedError(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedError(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Error);

	/// <inheritdoc cref="ThatFakeLogCollector.LoggedCritical(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedCritical(this IThat<FakeLogger?> subject)
		=> subject.Logged(LogLevel.Critical);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogTrace(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogTrace(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Trace);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogDebug(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogDebug(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Debug);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogInformation(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogInformation(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Information);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogWarning(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogWarning(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Warning);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogError(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogError(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Error);

	/// <inheritdoc cref="ThatFakeLogCollector.DidNotLogCritical(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogCritical(this IThat<FakeLogger?> subject)
		=> subject.DidNotLog(LogLevel.Critical);

	/// <summary>
	///     Registers the expectations on the collector with the expectation on the logger.
	/// </summary>
	/// <remarks>
	///     The value of the expectation stays the logger, so the constraints for it are forwarded its collector.
	/// </remarks>
	internal sealed class CollectorOfLogger(IThat<FakeLogger?> logger) : IExpectThat<FakeLogCollector?>
	{
		public ExpectationBuilder ExpectationBuilder { get; } = logger.Get().ExpectationBuilder;
	}
}
