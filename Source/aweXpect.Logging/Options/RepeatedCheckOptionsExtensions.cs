using System;

namespace aweXpect.Options;

internal static class RepeatedCheckOptionsExtensions
{
	/// <summary>
	///     Sets the <paramref name="interval" /> in which the records are read until the timeout.
	/// </summary>
	/// <remarks>
	///     As in aweXpect, where only the result of <c>Within</c> offers <c>CheckEvery</c>, the timeout must be
	///     specified before, as an interval without a timeout would be silently ignored.
	/// </remarks>
	/// <exception cref="InvalidOperationException">No timeout is specified yet.</exception>
	public static void CheckEveryAfterWithin(this RepeatedCheckOptions options, TimeSpan interval)
	{
		if (!options.IsRepeated)
		{
			throw new InvalidOperationException(
				"CheckEvery requires a timeout, which must be specified with Within before.");
		}

		options.CheckEvery(interval);
	}
}
