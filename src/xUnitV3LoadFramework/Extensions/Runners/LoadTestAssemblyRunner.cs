using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestAssemblyRunner :
	TestAssemblyRunner<LoadTestAssemblyRunnerContext, LoadTestAssembly, LoadCollection, LoadTestCase>
{
	public static LoadTestAssemblyRunner Instance { get; } = new();

	protected override ValueTask<string> GetTestFrameworkDisplayName(LoadTestAssemblyRunnerContext ctxt) =>
		new("Load Framework");

	public async ValueTask<RunSummary> Run(
		LoadTestAssembly testAssembly,
		IReadOnlyCollection<LoadTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions)
	{
		var cancellationToken = new CancellationToken();
		await using var ctxt = new LoadTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTestCollection(
		LoadTestAssemblyRunnerContext ctxt,
		LoadCollection testCollection,
		IReadOnlyCollection<LoadTestCase> testCases) =>
			LoadTestCollectionRunner.Instance.Run(
				testCollection,
				testCases,
				ctxt.MessageBus,
				ctxt.Aggregator.Clone(),
				ctxt.CancellationTokenSource
			);
}

public class LoadTestAssemblyRunnerContext(
	LoadTestAssembly testAssembly,
	IReadOnlyCollection<LoadTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		TestAssemblyRunnerContext<LoadTestAssembly, LoadTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{ }