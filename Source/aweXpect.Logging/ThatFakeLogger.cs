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
///     on its records, e.g. <c>HasNotLogged</c>, can follow.
/// </remarks>
public static class ThatFakeLogger
{
	/// <inheritdoc cref="ThatFakeLogCollector.HasLogged(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static HasLoggedResult HasLogged(this IThat<FakeLogger?> source)
		=> new CollectorOfLogger(source).HasLogged();

	/// <inheritdoc cref="ThatFakeLogCollector.HasLogged(IThat{FakeLogCollector}, LogLevel)" />
	[GuaranteesNotNull]
	public static HasLoggedLevelResult HasLogged(this IThat<FakeLogger?> source, LogLevel level)
		=> new CollectorOfLogger(source).HasLogged(level);

	/// <inheritdoc cref="ThatFakeLogCollector.HasNotLogged(IThat{FakeLogCollector})" />
	[GuaranteesNotNull]
	public static HasNotLoggedResult HasNotLogged(this IThat<FakeLogger?> source)
		=> new CollectorOfLogger(source).HasNotLogged();

	/// <inheritdoc cref="ThatFakeLogCollector.HasNotLogged(IThat{FakeLogCollector}, LogLevel)" />
	[GuaranteesNotNull]
	public static HasNotLoggedLevelResult HasNotLogged(this IThat<FakeLogger?> source, LogLevel level)
		=> new CollectorOfLogger(source).HasNotLogged(level);

	/// <summary>
	///     Registers the expectations on the collector with the expectation on the logger.
	/// </summary>
	/// <remarks>
	///     The value of the expectation stays the logger, which the constraint replaces by its collector.
	/// </remarks>
	private sealed class CollectorOfLogger(IThat<FakeLogger?> logger) : IExpectThat<FakeLogCollector?>
	{
		public ExpectationBuilder ExpectationBuilder { get; } = logger.Get().ExpectationBuilder;
	}
}
