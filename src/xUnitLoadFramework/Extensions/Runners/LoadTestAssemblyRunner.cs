using Xunit.Sdk;
using Xunit.v3;
using xUnitLoadFramework.Extensions.ObjectModel;

namespace xUnitLoadFramework.Extensions.Runners;

public class LoadTestAssemblyRunner :
	TestAssemblyRunner<LoadTestAssemblyRunnerContext, LoadTestAssembly, LoadTestCollection, LoadTestCase>
{
	public static LoadTestAssemblyRunner Instance { get; } = new();

	protected override ValueTask<string> GetTestFrameworkDisplayName(LoadTestAssemblyRunnerContext ctxt) =>
		new("Load Framework");

	public async ValueTask<RunSummary> Run(
		LoadTestAssembly testAssembly,
		IReadOnlyCollection<LoadTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken)
	{
		await using var ctxt = new LoadTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTestCollection(
		LoadTestAssemblyRunnerContext ctxt,
		LoadTestCollection testCollection,
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
