using aweXpect.Core;
using aweXpect.Options;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect;

public static partial class ThatFakeLogCollector
{
	/// <summary>
	///     Verifies that the subject did not log any record.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogResult DidNotLog(this IThat<FakeLogCollector?> source)
	{
		LogRecordFilter filter = new(null);
		RepeatedCheckOptions repeatedCheckOptions = new();
		return new DidNotLogResult(
			AddLoggedConstraint(source, filter, Quantifier.Never(), repeatedCheckOptions, []), source, filter,
			repeatedCheckOptions);
	}

	/// <summary>
	///     Verifies that the subject did not log a record with the <paramref name="level" />.
	/// </summary>
	[GuaranteesNotNull]
	public static DidNotLogLevelResult DidNotLog(this IThat<FakeLogCollector?> source, LogLevel level)
	{
		LogRecordFilter filter = new(level);
		RepeatedCheckOptions repeatedCheckOptions = new();
		return new DidNotLogLevelResult(
			AddLoggedConstraint(source, filter, Quantifier.Never(), repeatedCheckOptions, []), source, filter,
			repeatedCheckOptions);
	}
}
