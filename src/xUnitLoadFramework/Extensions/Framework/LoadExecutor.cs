using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace ObservationExample;

// We use ITestCase as our test case type, because we will see both ObservationTestCase objects
// as well as ExecutionErrorTestCase. ITestCase is the common denominator. We will end up dispatching
// the test cases appropriately in ObservationTestMethodRunner.
public class LoadExecutor(LoadTestAssembly testAssembly) :
    TestFrameworkExecutor<ITestCase>(testAssembly)
{
    public new LoadTestAssembly TestAssembly { get; } = testAssembly;

    protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
        new LoadDiscoverer(TestAssembly);

    public override async ValueTask RunTestCases(
        IReadOnlyCollection<ITestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken) =>
            await LoadTestAssemblyRunner.Instance.Run(TestAssembly, testCases.Cast<LoadTestCase>().ToArray(), executionMessageSink, executionOptions, cancellationToken);
}
