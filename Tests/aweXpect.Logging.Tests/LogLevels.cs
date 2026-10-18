using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace aweXpect.Logging.Tests;

internal static class LogLevels
{
	private static readonly LogLevel[] All =
	[
		LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical,
	];

	public static IEnumerable<LogLevel> Data() => All;

	public static IEnumerable<LogLevel> Except(LogLevel level)
		=> All.Where(l => l != level);
}
