using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using static aweXpect.Expect;

namespace aweXpect.Logging.Aot;

internal static partial class Checks
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
		new("a collector with a record of a higher level passes",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Critical, "Retry limit reached"))
				.HasLogged(LogLevel.Warning).OrHigher())),
		new("a collector with an unexpected record fails and lists the matching records",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasLogged(LogLevel.Warning).Never(),
				"Matching records:", "  [Warning] \"Retry limit reached\"")),
		new("a collector without a record of the level passes the absence check",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Information, "Starting"))
				.HasNotLogged(LogLevel.Warning).OrHigher())),
		new("a collector with a matching record fails the absence check",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasNotLogged().WithMessage("Retry *").AsWildcard(),
				"has not logged a record with message matching \"Retry *\"", "but it logged 1 such record")),
		new("a collector with a record with a derived exception passes",
			() => ShouldPass(async () =>
			{
				FakeLogCollector collector = new();
				FakeLogger logger = new(collector);
				FileNotFoundException exception = new();
				logger.LogError(exception, "Configuration missing");
				await That(collector).HasLogged(LogLevel.Error).WithException<IOException>();
			})),
		new("a collector without a record with the exception fails",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Error, "Retry limit reached"))
					.HasLogged(LogLevel.Error).WithException<TimeoutException>(),
				"has logged a record with level Error and exception of type TimeoutException",
				"  [Error] \"Retry limit reached\"")),
		new("a collector that logs the record during the timeout passes",
			() => ShouldPass(async () =>
			{
				FakeLogCollector collector = new();
				FakeLogger logger = new(collector);
				Task logging = Task.Run(async () =>
				{
					await Task.Delay(50);
					logger.LogWarning("Retry limit reached");
				});
				await That(collector).HasLogged(LogLevel.Warning).Within(TimeSpan.FromSeconds(30));
				await logging;
			})),
		new("a collector that logs an unexpected record during the timeout fails",
			() => ShouldFail(async () =>
				{
					FakeLogCollector collector = new();
					FakeLogger logger = new(collector);
					Task logging = Task.Run(async () =>
					{
						await Task.Delay(50);
						logger.LogError("Retry limit reached");
					});
					await That(collector).HasNotLogged(LogLevel.Error).Within(TimeSpan.FromSeconds(30));
					await logging;
				},
				"has not logged a record with level Error within 0:30", "but it logged 1 such record")),
		new("a collector with a record of the logger category of a nested generic type passes",
			() => ShouldPass(async () =>
			{
				FakeLogCollector collector = new();
				FakeLogger<Repository<int>> logger = new(collector);
				logger.LogInformation("Saving");
				await That(collector).HasLogged().WithCategory<Repository<string>>();
			})),
		new("a collector without a record of the event ID fails and lists the event IDs",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasLogged().WithEventId(1001),
				"has logged a record with event ID 1001", "  [Warning] \"Retry limit reached\" (event ID ")),
		new("a collector with a record of the property and template passes",
			() => ShouldPass(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
				.HasLogged().WithProperty("Message").WithValue("Retry limit *").AsWildcard()
				.WithMessageTemplate("{Message}"))),
		new("a collector without a record of the property fails and lists the property",
			() => ShouldFail(async () => await That(CollectorWith(LogLevel.Warning, "Retry limit reached"))
					.HasLogged().WithProperty("Attempt").WithValue("3"),
				"has logged a record with property Attempt with value equal to \"3\"",
				"  [Warning] \"Retry limit reached\" (no property Attempt)")),
		new("a collector with records in order passes",
			() => ShouldPass(async () =>
			{
				FakeLogCollector collector = new();
				FakeLogger logger = new(collector);
				logger.LogInformation("Starting");
				logger.LogWarning("Retry 1");
				await That(collector).HasLogged(LogLevel.Information)
					.Then(r => r.HasLogged(LogLevel.Warning).WithMessage("Retry *").AsWildcard());
			})),
		new("a collector with a record logged too early fails",
			() => ShouldFail(async () =>
				{
					FakeLogCollector collector = new();
					FakeLogger logger = new(collector);
					logger.LogWarning("Retry 1");
					logger.LogInformation("Starting");
					await That(collector).HasLogged(LogLevel.Information).Then(r => r.HasLogged(LogLevel.Warning));
				},
				"a record with level Warning in order", "but it logged a record with level Warning too early")),
	];

	private sealed class Repository<T>;

	private static FakeLogCollector CollectorWith(LogLevel level, string message)
	{
		FakeLogCollector collector = new();
		FakeLogger logger = new(collector);
		LogMessage(logger, level, message);
		return collector;
	}

	[LoggerMessage(Message = "{Message}")]
	private static partial void LogMessage(ILogger logger, LogLevel level, string message);

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
