using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestMethodRunner :
	TestMethodRunner<LoadTestMethodRunnerContext, LoadTestMethod, LoadTestCase>
{
	public static LoadTestMethodRunner Instance { get; } = new();

	public async ValueTask<RunSummary> Run(
		object testClassInstance,
		LoadTestMethod testMethod,
		IReadOnlyCollection<LoadTestCase> testCases,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		await using var ctxt = new LoadTestMethodRunnerContext(testClassInstance, testMethod, testCases, messageBus, aggregator, cancellationTokenSource);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTestCase(
		LoadTestMethodRunnerContext ctxt,
		LoadTestCase testCase) =>
		   LoadTestCaseRunner.Instance.Run(ctxt.TestClassInstance, testCase, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);
}

public class LoadTestMethodRunnerContext(
	object testClassInstance,
	LoadTestMethod testMethod,
	IReadOnlyCollection<LoadTestCase> testCases,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestMethodRunnerContext<LoadTestMethod, LoadTestCase>(testMethod, testCases, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{
	public object TestClassInstance { get; } = testClassInstance;
}
