using Xunit.Sdk;
using Xunit.v3;

namespace ObservationExample;

public class LoadTestAssemblyRunner :
	TestAssemblyRunner<ObservationTestAssemblyRunnerContext, LoadTestAssembly, LoadTestCollection, LoadTestCase>
{
	public static LoadTestAssemblyRunner Instance { get; } = new();

	protected override ValueTask<string> GetTestFrameworkDisplayName(ObservationTestAssemblyRunnerContext ctxt) =>
		new("Observation Framework");

	public async ValueTask<RunSummary> Run(
		LoadTestAssembly testAssembly,
		IReadOnlyCollection<LoadTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken)
	{
		await using var ctxt = new ObservationTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTestCollection(
		ObservationTestAssemblyRunnerContext ctxt,
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

public class ObservationTestAssemblyRunnerContext(
	LoadTestAssembly testAssembly,
	IReadOnlyCollection<LoadTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		TestAssemblyRunnerContext<LoadTestAssembly, LoadTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{ }
