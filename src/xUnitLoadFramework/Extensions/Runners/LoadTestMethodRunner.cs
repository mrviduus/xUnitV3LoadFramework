using Xunit.Sdk;
using Xunit.v3;
using xUnitLoadFramework.Extensions.ObjectModel;

namespace xUnitLoadFramework.Extensions.Runners;

public class LoadTestMethodRunner :
    TestMethodRunner<LoadTestMethodRunnerContext, LoadTestMethod, LoadTestCase>
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
        await using var ctxt = new LoadTestMethodRunnerContext(specification, testMethod, testCases, messageBus, aggregator, cancellationTokenSource);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    protected override ValueTask<RunSummary> RunTestCase(
        LoadTestMethodRunnerContext ctxt,
        LoadTestCase testCase) =>
           LoadTestCaseRunner.Instance.Run(ctxt.Specification, testCase, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);
}

public class LoadTestMethodRunnerContext(
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
