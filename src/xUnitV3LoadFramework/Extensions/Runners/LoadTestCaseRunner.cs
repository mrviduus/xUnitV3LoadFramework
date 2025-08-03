using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestCaseRunner :
	TestCaseRunner<LoadTestCaseRunnerContext, LoadTestCase, LoadTest>
{
	public static LoadTestCaseRunner Instance { get; } = new();

	public async ValueTask<RunSummary> Run(
		object testClassInstance,
		LoadTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		await using var ctxt = new LoadTestCaseRunnerContext(testClassInstance, testCase, messageBus, aggregator, cancellationTokenSource);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTest(
		LoadTestCaseRunnerContext ctxt,
		LoadTest test) =>
			LoadTestRunner.Instance.Run(ctxt.TestClassInstance, test, ctxt.MessageBus, test.TestCase.SkipReason, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);


}

public class LoadTestCaseRunnerContext(
	object testClassInstance,
	LoadTestCase testCase,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestCaseRunnerContext<LoadTestCase, LoadTest>(testCase, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{
	public object TestClassInstance { get; } = testClassInstance;

	public override IReadOnlyCollection<LoadTest> Tests =>
		[new LoadTest(TestCase)];
}
