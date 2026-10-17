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
		new("a collector with the expected number of records passes",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
				.HasLogged(LogLevel.Warning).Once())),
		new("a collector with another number of records fails",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasLogged(LogLevel.Warning).Twice(),
				"has logged a record with level Warning exactly twice", "but it logged 1 such record")),
		new("a collector with a record matching the message pattern passes",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit 3 reached"))
				.HasLogged().WithMessage("Retry limit * reached").AsWildcard())),
		new("a collector without a record matching the message regex fails",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasLogged(LogLevel.Warning).WithMessage("^Retry [0-9]+$").AsRegex(),
				"has logged a record with level Warning and message matching regex \"^Retry [0-9]+$\"",
				"but it did not log any such record")),
	];

	private static FakeLogCollector CollectorWith(LogLevel level, string message)
	{
		FakeLogCollector collector = new();
		new FakeLogger(collector).Log(level, "{Message}", message);
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
