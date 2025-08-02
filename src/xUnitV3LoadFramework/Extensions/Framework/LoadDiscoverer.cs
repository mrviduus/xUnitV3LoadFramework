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
		// Check for both StressAttribute (preferred) and LoadAttribute (backward compatibility)
		var stressAttribute = testMethod.Method.GetCustomAttributes<StressAttribute>().FirstOrDefault();
		var loadAttribute = testMethod.Method.GetCustomAttributes<LoadAttribute>().FirstOrDefault();
		
		// Use StressAttribute if present, otherwise fall back to LoadAttribute
		var attribute = stressAttribute ?? loadAttribute;
		if (attribute is null)
			return true;

		var order = attribute.Order;
		var concurrency = attribute.Concurrency;
		var duration = attribute.Duration;
		var interval = attribute.Interval;
		var skipReason = attribute.Skip;

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
				sourceFilePath = sourceFilePathProp.GetValue(attribute) as string;
			if (sourceLineNumberProp != null)
				sourceLineNumber = sourceLineNumberProp.GetValue(attribute) as int?;
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
		// Check if the class should use stress/load framework (handle both new and deprecated attributes)
		var useStressFramework = testClass.Class.GetCustomAttributes<UseStressFrameworkAttribute>().Any();
		var useLoadFramework = testClass.Class.GetCustomAttributes<UseLoadFrameworkAttribute>().Any();
		
		var useFramework = useStressFramework || useLoadFramework;
		
		if (useFramework)
		{
			// V2: No longer require Specification inheritance - support plain classes
			// Process methods looking for [Stress] or [Load] attributes
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
