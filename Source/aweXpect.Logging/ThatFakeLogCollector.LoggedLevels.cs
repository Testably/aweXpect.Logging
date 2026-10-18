using aweXpect.Core;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect;

public static partial class ThatFakeLogCollector
{
	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Trace" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedTrace(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Trace);

	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Debug" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedDebug(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Debug);

	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Information" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedInformation(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Information);

	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Warning" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedWarning(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Warning);

	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Error" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedError(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Error);

	/// <summary>
	///     Verifies that the subject logged a record with the level <see cref="LogLevel.Critical" />, by default at
	///     least once.
	/// </summary>
	[GuaranteesNotNull]
	public static LoggedLevelResult LoggedCritical(this IThat<FakeLogCollector?> subject)
		=> subject.Logged(LogLevel.Critical);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Trace" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogTrace(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Trace);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Debug" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogDebug(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Debug);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Information" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogInformation(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Information);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Warning" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogWarning(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Warning);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Error" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogError(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Error);

	/// <summary>
	///     Verifies that the subject did not log a record with the level <see cref="LogLevel.Critical" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLogCritical(this IThat<FakeLogCollector?> subject)
		=> subject.DidNotLog(LogLevel.Critical);
}
