using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace aweXpect.Logging.Tests;

public sealed partial class ThatFakeLogCollector
{
	public sealed partial class HasLogged
	{
		public sealed class WithCategoryTests
		{
			[Fact]
			public async Task Generic_ForNestedGenericType_ShouldMatchTheCategoryOfTheLogger()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger<Outer.Repository<int>>();
				logger.LogInformation("Saving");

				async Task Act()
					=> await That(subject).HasLogged().WithCategory<Outer.Repository<string>>();

				await That(Act).DoesNotThrow()
					.Because("the category of a generic type is its name without the type arguments");
			}

			[Fact]
			public async Task Generic_WhenLoggedByFakeLoggerOfTheType_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				FakeLogger<OrderService> logger = new(subject);
				logger.LogInformation("Placing order");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).WithCategory<OrderService>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task Generic_WhenLoggedByLoggerOfTheType_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger<OrderService> logger = factory.CreateLogger<OrderService>();
				logger.LogInformation("Placing order");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).WithCategory<OrderService>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task Generic_WhenLoggedByLoggerOfOtherType_ShouldFail()
			{
				FakeLogCollector subject = new();
				FakeLogger<PaymentService> logger = new(subject);
				logger.LogInformation("Placing order");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Information).WithCategory<OrderService>();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Information and category "aweXpect.Logging.Tests.ThatFakeLogCollector.HasLogged.WithCategoryTests.OrderService" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Information] "Placing order" (category "aweXpect.Logging.Tests.ThatFakeLogCollector.HasLogged.WithCategoryTests.PaymentService")
					             ]
					             """);
			}

			[Fact]
			public async Task WhenCategoryDiffersInCase_ShouldFail()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Orders");
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithCategory("myapp.orders");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with level Warning and category "myapp.orders" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry limit reached" (category "MyApp.Orders")
					             ]
					             """)
					.Because("the category is compared exactly");
			}

			[Fact]
			public async Task WhenCategoryIsEqual_ShouldSucceed()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Orders");
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged(LogLevel.Warning).WithCategory("MyApp.Orders");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNegated_AndCategoryMatches_ShouldFail()
			{
				FakeLogCollector subject = new();
				using LoggerFactory factory = new([new FakeLoggerProvider(subject),]);
				ILogger logger = factory.CreateLogger("MyApp.Orders");
				logger.LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasLogged().WithCategory("MyApp.Orders"));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has not logged a record with category "MyApp.Orders",
					             but it logged 1 such record

					             Matching records:
					             [
					               [Warning] "Retry limit reached" (category "MyApp.Orders")
					             ]

					             Records:
					             [
					               [Warning] "Retry limit reached" (category "MyApp.Orders")
					             ]
					             """);
			}

			[Fact]
			public async Task WhenRecordHasNoCategory_ShouldFail()
			{
				FakeLogCollector subject = new();
				new FakeLogger(subject).LogWarning("Retry limit reached");

				async Task Act()
					=> await That(subject).HasLogged().WithCategory("MyApp.Orders");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has logged a record with category "MyApp.Orders" at least once,
					             but it did not log any such record

					             Records:
					             [
					               [Warning] "Retry limit reached" (category <null>)
					             ]
					             """);
			}

			public sealed class OrderService;

			public sealed class Outer
			{
				public sealed class Repository<T>;
			}

			public sealed class PaymentService;
		}
	}
}
