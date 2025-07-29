// Import System.Reflection for assembly metadata access
using System.Reflection;
// Import xUnit v3 framework for test framework integration
using Xunit.v3;
// Import custom object model for load testing components
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

/// <summary>
/// Custom test framework that extends xUnit v3 to provide load testing capabilities.
/// Replaces standard xUnit test discovery and execution with load-aware implementations
/// that support performance monitoring, result aggregation, and actor-based execution.
/// </summary>
public class LoadTestFramework : TestFramework
{
	/// <summary>
	/// Gets the display name for this test framework as shown in test runners and reports.
	/// Identifies this framework as the "Load Framework" in test output and tooling.
	/// </summary>
	public override string TestFrameworkDisplayName =>
		"Load Framework";

	/// <summary>
	/// Creates a test discoverer for finding and categorizing load tests within an assembly.
	/// The discoverer identifies methods marked with [Load] attributes and creates appropriate test cases.
	/// </summary>
	/// <param name="assembly">The assembly to scan for load tests</param>
	/// <returns>A LoadDiscoverer configured for the specified assembly</returns>
	protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly) =>
		// Create a new LoadDiscoverer with a LoadTestAssembly wrapper
		// This enables load-specific test discovery and categorization
		new LoadDiscoverer(new LoadTestAssembly(assembly));

	/// <summary>
	/// Creates a test executor for running discovered load tests with performance monitoring.
	/// The executor orchestrates actor-based load test execution with result collection.
	/// </summary>
	/// <param name="assembly">The assembly containing load tests to execute</param>
	/// <returns>A LoadExecutor configured for the specified assembly</returns>
	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
		// Create a new LoadExecutor with a LoadTestAssembly wrapper
		// This enables load-specific test execution with performance tracking
		new LoadExecutor(new LoadTestAssembly(assembly));
}
