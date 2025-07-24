using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

public class LoadDiscoverer(LoadTestAssembly testAssembly) :
	TestFrameworkDiscoverer<LoadTestClass>(testAssembly)
{
	public new LoadTestAssembly TestAssembly { get; } = testAssembly;

	protected override ValueTask<LoadTestClass> CreateTestClass(Type @class) =>
		new(new LoadTestClass(TestAssembly, @class));

	static async ValueTask<bool> FindTestsForMethod(
		LoadTestMethod testMethod,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<LoadTestCase, ValueTask<bool>> discoveryCallback)
	{
		var loadAttribute = testMethod.Method.GetCustomAttributes<LoadAttribute>().FirstOrDefault();
		if (loadAttribute is null)
			return true;

		var order = loadAttribute.Order;
		var cuncurrency = loadAttribute.Concurrency;
		var duration = loadAttribute.Duration;
		var interval = loadAttribute.Interval;
		var skipReason = loadAttribute.Skip;

		var testCase = new LoadTestCase(testMethod, order);
		testCase.Concurrency = cuncurrency;
		testCase.Duration = duration;
		testCase.Interval = interval;
		testCase.SkipReason = skipReason;
		//var testCase = new LoadTestCase(testMethod, cuncurrency, duration, interval);
		if (!await discoveryCallback(testCase))
			return false;

		return true;
	}

	protected override async ValueTask<bool> FindTestsForType(
		LoadTestClass testClass,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<ITestCase, ValueTask<bool>> discoveryCallback)
	{
		// Check if the class should use load framework
		var useLoadFramework = testClass.Class.GetCustomAttributes<UseLoadFrameworkAttribute>().Any();
		
		if (useLoadFramework)
		{
			// For load framework classes, only process classes that inherit from Specification
			if (!typeof(Specification).IsAssignableFrom(testClass.Class))
				return true;

			// Process methods looking for [Load] attributes
			foreach (var method in testClass.Methods)
			{
				var testMethod = new LoadTestMethod(testClass, method);

				try
				{
					if (!await FindTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
						return false;
				}
				catch (Exception ex)
				{
					TestContext.Current.SendDiagnosticMessage("Exception during discovery of test class {0}:{1}{2}", testClass.Class.FullName, Environment.NewLine, ex);
				}
			}
		}
		else
		{
			// For classes without [UseLoadFramework], discover standard xUnit tests
			foreach (var method in testClass.Methods)
			{
				var testMethod = new LoadTestMethod(testClass, method);

				try
				{
					if (!await FindStandardTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
						return false;
				}
				catch (Exception ex)
				{
					TestContext.Current.SendDiagnosticMessage("Exception during discovery of test class {0}:{1}{2}", testClass.Class.FullName, Environment.NewLine, ex);
				}
			}
		}
		
		return true;
	}

	static async ValueTask<bool> FindStandardTestsForMethod(
		LoadTestMethod testMethod,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<ITestCase, ValueTask<bool>> discoveryCallback)
	{
		// Check for [Fact] attribute
		var factAttribute = testMethod.Method.GetCustomAttributes<FactAttribute>().FirstOrDefault();
		if (factAttribute is not null)
		{
			var testCase = new StandardTestCase(testMethod, factAttribute);
			return await discoveryCallback(testCase);
		}

		// Check for [Theory] attribute (which inherits from FactAttribute)
		var theoryAttribute = testMethod.Method.GetCustomAttributes<TheoryAttribute>().FirstOrDefault();
		if (theoryAttribute is not null)
		{
			var testCase = new StandardTestCase(testMethod, theoryAttribute);
			return await discoveryCallback(testCase);
		}

		return true;
	}

	protected override Type[] GetExportedTypes() =>
		TestAssembly.Assembly.ExportedTypes.ToArray();
}
