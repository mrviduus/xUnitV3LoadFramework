using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;
using xUnitV3LoadFramework.Extensions.Runners;

namespace xUnitV3LoadFramework.Extensions.Framework;

// We use ITestCase as our test case type, because we will see both LoadTestCase objects
// as well as ExecutionErrorTestCase. ITestCase is the common denominator. We will end up dispatching
// the test cases appropriately in LoadTestMethodRunner.
public class LoadExecutor(LoadTestAssembly testAssembly) :
	TestFrameworkExecutor<ITestCase>(testAssembly)
{
	public new LoadTestAssembly TestAssembly { get; } = testAssembly;

	protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
		new LoadDiscoverer(TestAssembly);

	public override async ValueTask RunTestCases(
		IReadOnlyCollection<ITestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken) =>
			await LoadTestAssemblyRunner.Instance.Run(TestAssembly, testCases.Cast<LoadTestCase>().ToArray(), executionMessageSink, executionOptions);
}
