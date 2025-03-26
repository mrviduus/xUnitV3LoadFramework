using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestAssemblyRunner :
    TestAssemblyRunner<ObservationTestAssemblyRunnerContext, LoadTestAssembly, LoadCollection, LoadTestCase>
{
    public static LoadTestAssemblyRunner Instance { get; } = new();

    protected override ValueTask<string> GetTestFrameworkDisplayName(ObservationTestAssemblyRunnerContext ctxt) =>
        new("Observation Framework");

    public async ValueTask<RunSummary> Run(
        LoadTestAssembly testAssembly,
        IReadOnlyCollection<LoadTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        var cancellationToken = new CancellationToken();
        await using var ctxt = new ObservationTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    protected override ValueTask<RunSummary> RunTestCollection(
        ObservationTestAssemblyRunnerContext ctxt,
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

public class ObservationTestAssemblyRunnerContext(
    LoadTestAssembly testAssembly,
    IReadOnlyCollection<LoadTestCase> testCases,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions,
    CancellationToken cancellationToken) :
        TestAssemblyRunnerContext<LoadTestAssembly, LoadTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{ }