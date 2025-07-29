// Import xUnit SDK for test framework execution infrastructure
using Xunit.Sdk;
// Import xUnit v3 core interfaces for test execution
using Xunit.v3;
// Import custom object model for load testing components
using xUnitV3LoadFramework.Extensions.ObjectModel;
// Import custom runners for load test execution orchestration
using xUnitV3LoadFramework.Extensions.Runners;

namespace xUnitV3LoadFramework.Extensions.Framework;

/// <summary>
/// Custom test framework executor that orchestrates load test execution.
/// Handles both LoadTestCase objects and ExecutionErrorTestCase objects,
/// dispatching them to appropriate runners for performance-aware execution.
/// Uses ITestCase as the common denominator for different test case types.
/// </summary>
public class LoadExecutor(LoadTestAssembly testAssembly) :
	TestFrameworkExecutor<ITestCase>(testAssembly)
{
	/// <summary>
	/// Gets the LoadTestAssembly that contains the load tests to execute.
	/// Provides strongly-typed access to the test assembly with load testing metadata.
	/// </summary>
	public new LoadTestAssembly TestAssembly { get; } = testAssembly;

	/// <summary>
	/// Creates a test discoverer for finding load tests within the assembly.
	/// The discoverer identifies and categorizes tests for load execution.
	/// </summary>
	/// <returns>A LoadDiscoverer configured for this assembly</returns>
	protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
		// Create a new LoadDiscoverer using the same LoadTestAssembly instance
		// This ensures consistency between discovery and execution phases
		new LoadDiscoverer(TestAssembly);

	/// <summary>
	/// Executes a collection of test cases using the load testing framework.
	/// Coordinates the execution of load tests with performance monitoring and result aggregation.
	/// </summary>
	/// <param name="testCases">Collection of test cases to execute (LoadTestCase and ExecutionErrorTestCase)</param>
	/// <param name="executionMessageSink">Sink for test execution messages and results</param>
	/// <param name="executionOptions">Options controlling test execution behavior</param>
	/// <param name="cancellationToken">Token for cancelling test execution</param>
	/// <returns>A task representing the asynchronous test execution operation</returns>
	public override async ValueTask RunTestCases(
		IReadOnlyCollection<ITestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions, 
		CancellationToken cancellationToken) =>
			// Delegate to LoadTestAssemblyRunner for coordinated execution
			// Cast test cases to LoadTestCase for load-specific handling
			await LoadTestAssemblyRunner.Instance.Run(
				TestAssembly, 
				testCases.Cast<LoadTestCase>().ToArray(), 
				executionMessageSink, 
				executionOptions);
}
