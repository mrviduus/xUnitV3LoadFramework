using Xunit.Sdk;
using Xunit.v3;

namespace ObservationExample;

public class LoadTestClassRunner :
	TestClassRunner<ObservationTestClassRunnerContext, LoadTestClass, LoadTestMethod, LoadTestCase>
{
	public static LoadTestClassRunner Instance { get; } = new();

	protected override IReadOnlyCollection<LoadTestCase> OrderTestCases(ObservationTestClassRunnerContext ctxt) =>
		[.. ctxt.TestCases.OrderBy(tc => tc.Order)];

	public async ValueTask<RunSummary> Run(
		Specification specification,
		LoadTestClass testClass,
		IReadOnlyCollection<LoadTestCase> testCases,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		await using var ctxt = new ObservationTestClassRunnerContext(specification, testClass, testCases, messageBus, aggregator, cancellationTokenSource);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	protected override ValueTask<RunSummary> RunTestMethod(
		ObservationTestClassRunnerContext ctxt,
		LoadTestMethod? testMethod,
		IReadOnlyCollection<LoadTestCase> testCases,
		object?[] constructorArguments)
	{
		ArgumentNullException.ThrowIfNull(testMethod);

		return LoadTestMethodRunner.Instance.Run(ctxt.Specification, testMethod, testCases, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);
	}
}

public class ObservationTestClassRunnerContext(
	Specification specification,
	LoadTestClass testClass,
	IReadOnlyCollection<LoadTestCase> testCases,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestClassRunnerContext<LoadTestClass, LoadTestCase>(testClass, testCases, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{
	public Specification Specification { get; } = specification;
}
