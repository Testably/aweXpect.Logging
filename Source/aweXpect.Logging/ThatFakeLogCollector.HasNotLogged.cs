using aweXpect.Core;
using aweXpect.Options;
using aweXpect.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect;

public static partial class ThatFakeLogCollector
{
	/// <summary>
	///     Verifies that the subject has not logged any record.
	/// </summary>
	[GuaranteesNotNull]
	public static HasNotLoggedResult HasNotLogged(this IThat<FakeLogCollector?> source)
	{
		LogRecordFilter filter = new(null);
		RepeatedCheckOptions repeatedCheckOptions = new();
		return new HasNotLoggedResult(
			AddHasLoggedConstraint(source, filter, Quantifier.Never(), repeatedCheckOptions), source, filter,
			repeatedCheckOptions);
	}

	/// <summary>
	///     Verifies that the subject has not logged a record with the <paramref name="level" />.
	/// </summary>
	[GuaranteesNotNull]
	public static HasNotLoggedLevelResult HasNotLogged(this IThat<FakeLogCollector?> source, LogLevel level)
	{
		LogRecordFilter filter = new(level);
		RepeatedCheckOptions repeatedCheckOptions = new();
		return new HasNotLoggedLevelResult(
			AddHasLoggedConstraint(source, filter, Quantifier.Never(), repeatedCheckOptions), source, filter,
			repeatedCheckOptions);
	}
}
