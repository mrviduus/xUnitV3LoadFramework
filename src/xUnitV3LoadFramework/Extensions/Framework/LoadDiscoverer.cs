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
		var concurrency = loadAttribute.Concurrency;
		var duration = loadAttribute.Duration;
		var interval = loadAttribute.Interval;
		var skipReason = loadAttribute.Skip;

		// For xUnit v3, source location information should be automatically provided via CallerFilePath/CallerLineNumber
		// These are set in the LoadAttribute constructor and will be available through the FactAttribute base class
		string? sourceFilePath = null;
		int? sourceLineNumber = null;

		// Try to extract source info using reflection if available
		try
		{
			var sourceFilePathProp = typeof(FactAttribute).GetProperty("SourceFilePath");
			var sourceLineNumberProp = typeof(FactAttribute).GetProperty("SourceLineNumber");
			
			if (sourceFilePathProp != null)
				sourceFilePath = sourceFilePathProp.GetValue(loadAttribute) as string;
			if (sourceLineNumberProp != null)
				sourceLineNumber = sourceLineNumberProp.GetValue(loadAttribute) as int?;
		}
		catch
		{
			// If reflection fails, continue without source location
		}

		var testCase = new LoadTestCase(testMethod, order, sourceFilePath, sourceLineNumber);
		testCase.Concurrency = concurrency;
		testCase.Duration = duration;
		testCase.Interval = interval;
		testCase.SkipReason = skipReason;
		
		if (!await discoveryCallback(testCase))
			return false;

		return true;
	}

	protected override async ValueTask<bool> FindTestsForType(
		LoadTestClass testClass,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<ITestCase, ValueTask<bool>> discoveryCallback)
	{
		// Process all methods looking for [Load] attributes or standard xUnit attributes
		foreach (var method in testClass.Methods)
		{
			var testMethod = new LoadTestMethod(testClass, method);

			try
			{
				// Check if method has [Load] attribute first
				var loadAttribute = testMethod.Method.GetCustomAttributes<LoadAttribute>().FirstOrDefault();
				if (loadAttribute is not null)
				{
					// Process as load test
					if (!await FindTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
						return false;
				}
				else
				{
					// Process as standard xUnit test
					if (!await FindStandardTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
						return false;
				}
			}
			catch (Exception ex)
			{
				TestContext.Current.SendDiagnosticMessage("Exception during discovery of test class {0}:{1}{2}", testClass.Class.FullName, Environment.NewLine, ex);
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
			// Extract source location information from FactAttribute using reflection
			string? sourceFilePath = null;
			int? sourceLineNumber = null;

			try
			{
				var sourceFilePathProp = typeof(FactAttribute).GetProperty("SourceFilePath");
				var sourceLineNumberProp = typeof(FactAttribute).GetProperty("SourceLineNumber");
				
				if (sourceFilePathProp != null)
					sourceFilePath = sourceFilePathProp.GetValue(factAttribute) as string;
				if (sourceLineNumberProp != null)
					sourceLineNumber = sourceLineNumberProp.GetValue(factAttribute) as int?;
			}
			catch
			{
				// If reflection fails, continue without source location
			}

			var testCase = new StandardTestCase(testMethod, factAttribute, sourceFilePath ?? "", sourceLineNumber ?? 0);
			return await discoveryCallback(testCase);
		}

		// Check for [Theory] attribute (which inherits from FactAttribute)
		var theoryAttribute = testMethod.Method.GetCustomAttributes<TheoryAttribute>().FirstOrDefault();
		if (theoryAttribute is not null)
		{
			// Extract source location information from TheoryAttribute using reflection
			string? sourceFilePath = null;
			int? sourceLineNumber = null;

			try
			{
				var sourceFilePathProp = typeof(FactAttribute).GetProperty("SourceFilePath");
				var sourceLineNumberProp = typeof(FactAttribute).GetProperty("SourceLineNumber");
				
				if (sourceFilePathProp != null)
					sourceFilePath = sourceFilePathProp.GetValue(theoryAttribute) as string;
				if (sourceLineNumberProp != null)
					sourceLineNumber = sourceLineNumberProp.GetValue(theoryAttribute) as int?;
			}
			catch
			{
				// If reflection fails, continue without source location
			}

			var testCase = new StandardTestCase(testMethod, theoryAttribute, sourceFilePath ?? "", sourceLineNumber ?? 0);
			return await discoveryCallback(testCase);
		}

		return true;
	}

	protected override Type[] GetExportedTypes() =>
		TestAssembly.Assembly.ExportedTypes.ToArray();
}
