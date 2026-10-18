using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using aweXpect.Results;

namespace aweXpect.Logging.Tests;

public sealed class ResultFamilyParityTests
{
	private static readonly Type[] Roots =
	[
		typeof(LoggedResult), typeof(DidNotLogResult), typeof(LogRecordStepResult),
	];

	[Test]
	[Arguments(typeof(LoggedResult), typeof(DidNotLogResult), typeof(LogRecordStepResult))]
	[Arguments(typeof(LoggedCategoryResult), typeof(DidNotLogCategoryResult), typeof(LogRecordStepCategoryResult))]
	[Arguments(typeof(LoggedLevelResult), typeof(DidNotLogLevelResult), typeof(LogRecordStepLevelResult))]
	[Arguments(typeof(LoggedMessageResult), typeof(DidNotLogMessageResult), typeof(LogRecordStepMessageResult))]
	[Arguments(typeof(LoggedPropertyResult), typeof(DidNotLogPropertyResult), typeof(LogRecordStepPropertyResult))]
	[Arguments(typeof(LoggedPropertyValueResult), typeof(DidNotLogPropertyValueResult),
		typeof(LogRecordStepPropertyValueResult))]
	public async Task Filters_ShouldBeTheSameInEveryFamily(Type logged, Type didNotLog, Type step)
	{
		Type[] family = [logged, didNotLog, step,];

		string[] missing = Missing(family, Filters);

		await That(missing).IsEmpty()
			.Because(
				"every With* and OrHigher filter (with the same name, generic arity, parameters and return type relative to its family) must exist on Logged(), DidNotLog() and the steps in Then(...)");
	}

	[Test]
	[Arguments(typeof(LoggedCategoryResult), typeof(DidNotLogCategoryResult), typeof(LogRecordStepCategoryResult))]
	[Arguments(typeof(LoggedLevelResult), typeof(DidNotLogLevelResult), typeof(LogRecordStepLevelResult))]
	[Arguments(typeof(LoggedMessageResult), typeof(DidNotLogMessageResult), typeof(LogRecordStepMessageResult))]
	[Arguments(typeof(LoggedPropertyResult), typeof(DidNotLogPropertyResult), typeof(LogRecordStepPropertyResult))]
	[Arguments(typeof(LoggedPropertyValueResult), typeof(DidNotLogPropertyValueResult),
		typeof(LogRecordStepPropertyValueResult))]
	public async Task OptionInterfaces_ShouldBeTheSameInEveryFamily(Type logged, Type didNotLog, Type step)
	{
		Type[] family = [logged, didNotLog, step,];

		string[] missing = Missing(family, OptionInterfaces);

		await That(missing).IsEmpty()
			.Because(
				"a filter must offer the same options on Logged(), DidNotLog() and the steps in Then(...)");
	}

	private static IEnumerable<string> AllFilters(Type type)
		=> type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.DeclaringType!.Assembly == typeof(LoggedResult).Assembly)
			.Where(m => m.Name == "OrHigher" || (m.Name.Length > 4 &&
			                                     m.Name.StartsWith("With", StringComparison.Ordinal) &&
			                                     char.IsUpper(m.Name[4])))
			.Select(m => $"{m.Name}{(m.IsGenericMethodDefinition ? $"<{m.GetGenericArguments().Length}>" : "")}"
			             + $"({string.Join(", ", m.GetParameters().Select(p => Name(p.ParameterType)))})"
			             + $" -> {RelativeToFamily(m.ReturnType)}");

	/// <remarks>
	///     A sub-result only contributes what it adds to its family root, so that a filter missing on a root is
	///     reported once.
	/// </remarks>
	private static IEnumerable<string> Filters(Type type)
	{
		Type root = Root(type);
		return root == type ? AllFilters(type) : AllFilters(type).Except(AllFilters(root));
	}

	private static string[] Missing(Type[] family, Func<Type, IEnumerable<string>> members)
	{
		Dictionary<Type, HashSet<string>> membersByType =
			family.ToDictionary(t => t, t => new HashSet<string>(members(t)));
		return membersByType.Values.SelectMany(m => m).Distinct().OrderBy(m => m, StringComparer.Ordinal)
			.SelectMany(member => family
				.Where(type => !membersByType[type].Contains(member))
				.Select(type => $"{type.Name} misses {member}"))
			.ToArray();
	}

	private static string Name(Type type)
		=> type.IsGenericType
			? $"{type.Name.Substring(0, type.Name.IndexOf('`'))}<{string.Join(", ", type.GetGenericArguments().Select(Name))}>"
			: type.Name;

	private static IEnumerable<string> OptionInterfaces(Type type)
		=> type.GetInterfaces()
			.Except(Root(type).GetInterfaces())
			.Select(Name);

	private static string RelativeToFamily(Type type)
	{
		foreach (string prefix in new[] { "LogRecordStep", "DidNotLog", "Logged", })
		{
			if (type.Assembly == typeof(LoggedResult).Assembly &&
			    type.Name.StartsWith(prefix, StringComparison.Ordinal))
			{
				return $"{{family}}{type.Name.Substring(prefix.Length)}";
			}
		}

		return Name(type);
	}

	private static Type Root(Type type)
		=> Roots.Single(root => root.IsAssignableFrom(type));
}
