using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using static aweXpect.Expect;

namespace aweXpect.Logging.Aot;

internal static class Checks
{
	public static readonly Check[] All =
	[
		new("a collector with a record of the level passes",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
				.HasLogged(LogLevel.Warning))),
		new("a collector without a record of the level fails and lists the records",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Information, "Starting"))
					.HasLogged(LogLevel.Warning),
				"has logged a record with level Warning", "  [Information] \"Starting\"")),
	];

	private static FakeLogCollector CollectorWith(LogLevel level, string message)
	{
		FakeLogCollector collector = new();
		new FakeLogger(collector).Log(level, message);
		return collector;
	}

	private static async Task<string?> ShouldFail(Func<Task> act, params string[] parts)
	{
		try
		{
			await act();
		}
		catch (FailException exception)
		{
			string? missing = Array.Find(parts, part => !exception.Message.Contains(part, StringComparison.Ordinal));
			return missing is null ? null : $"message lacks \"{missing}\": {exception.Message}";
		}
		catch (Exception exception)
		{
			return $"threw {exception.GetType().FullName} instead of {typeof(FailException).FullName}: {exception.Message}";
		}

		return "did not throw";
	}

	private static async Task<string?> ShouldPass(Func<Task> act)
	{
		try
		{
			await act();
			return null;
		}
		catch (Exception exception)
		{
			return $"threw {exception.GetType().FullName}: {exception.Message}";
		}
	}
}
