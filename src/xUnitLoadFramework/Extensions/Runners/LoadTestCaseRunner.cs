using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace ObservationExample;

public class LoadTestCaseRunner :
    TestCaseRunner<ObservationTestCaseRunnerContext, LoadTestCase, LoadTest>
{
    public static LoadTestCaseRunner Instance { get; } = new();

    public async ValueTask<RunSummary> Run(
        Specification specification,
        LoadTestCase testCase,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        await using var ctxt = new ObservationTestCaseRunnerContext(specification, testCase, messageBus, aggregator, cancellationTokenSource);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    protected override ValueTask<RunSummary> RunTest(
        ObservationTestCaseRunnerContext ctxt,
        LoadTest test) =>
            LoadTestRunner.Instance.Run(ctxt.Specification, test, ctxt.MessageBus, null, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);
}

public class ObservationTestCaseRunnerContext(
    Specification specification,
    LoadTestCase testCase,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) :
        TestCaseRunnerContext<LoadTestCase, LoadTest>(testCase, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{
    public Specification Specification { get; } = specification;

    public override IReadOnlyCollection<LoadTest> Tests =>
        [new LoadTest(TestCase)];
}
