using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestMethodRunner :
    TestMethodRunner<ObservationTestMethodRunnerContext, LoadTestMethod, LoadTestCase>
{
    public static LoadTestMethodRunner Instance { get; } = new();

    public async ValueTask<RunSummary> Run(
        Specification specification,
        LoadTestMethod testMethod,
        IReadOnlyCollection<LoadTestCase> testCases,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        await using var ctxt = new ObservationTestMethodRunnerContext(specification, testMethod, testCases, messageBus, aggregator, cancellationTokenSource);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    protected override ValueTask<RunSummary> RunTestCase(
        ObservationTestMethodRunnerContext ctxt,
        LoadTestCase testCase) =>
           LoadTestCaseRunner.Instance.Run(ctxt.Specification, testCase, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);
}

public class ObservationTestMethodRunnerContext(
    Specification specification,
    LoadTestMethod testMethod,
    IReadOnlyCollection<LoadTestCase> testCases,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) :
        TestMethodRunnerContext<LoadTestMethod, LoadTestCase>(testMethod, testCases, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{
    public Specification Specification { get; } = specification;
}
