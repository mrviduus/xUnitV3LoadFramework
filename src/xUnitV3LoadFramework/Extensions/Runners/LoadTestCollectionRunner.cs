using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;
using xUnitV3LoadFramework.Extensions.Framework;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestCollectionRunner :
    TestCollectionRunner<LoadTestCollectionRunnerContext, LoadCollection, LoadTestClass, LoadTestCase>
{
    public static LoadTestCollectionRunner Instance { get; } = new();

    protected override ValueTask<RunSummary> FailTestClass(LoadTestCollectionRunnerContext ctxt, LoadTestClass? testClass, IReadOnlyCollection<LoadTestCase> testCases, Exception exception)
    {
        var result = XunitRunnerHelper.FailTestCases(
            Guard.ArgumentNotNull(ctxt).MessageBus,
            ctxt.CancellationTokenSource,
            testCases,
            exception,
            sendTestClassMessages: true,
            sendTestMethodMessages: true
        );

        return new(result);
    }

    protected override async ValueTask<RunSummary> RunTestClass(
        LoadTestCollectionRunnerContext ctxt,
        LoadTestClass? testClass,
        IReadOnlyCollection<LoadTestCase> testCases)
    {
        ArgumentNullException.ThrowIfNull(testClass);

        // Check if we have standard test cases (non-load tests)
        var standardTestCases = testCases.OfType<StandardTestCase>().ToArray();
        var loadTestCases = testCases.Where(tc => tc is not StandardTestCase).ToArray();

        var totalResult = new RunSummary();

        // Handle standard test cases
        if (standardTestCases.Length > 0)
        {
            var standardResult = await RunStandardTestClass(ctxt, testClass, standardTestCases);
            totalResult.Aggregate(standardResult);
        }

        // Handle load test cases
        if (loadTestCases.Length > 0)
        {
            var loadResult = await RunLoadTestClass(ctxt, testClass, loadTestCases);
            totalResult.Aggregate(loadResult);
        }

        return totalResult;
    }

    private async ValueTask<RunSummary> RunStandardTestClass(
        LoadTestCollectionRunnerContext ctxt,
        LoadTestClass testClass,
        StandardTestCase[] testCases)
    {
        object? testClassInstance = null;

        // Create instance of the standard test class (doesn't need to inherit from Specification)
        try
        {
            testClassInstance = Activator.CreateInstance(testClass.Class);
        }
        catch (Exception ex)
        {
            return await FailTestClass(ctxt, testClass, testCases.Cast<LoadTestCase>().ToArray(), ex);
        }

        if (testClassInstance == null)
        {
            return await FailTestClass(ctxt, testClass, testCases.Cast<LoadTestCase>().ToArray(), 
                new InvalidOperationException("Failed to create instance of test class"));
        }

        // Run standard test cases using a simple runner
        return await RunStandardTestCases(ctxt, testClass, testClassInstance, testCases);
    }

    private async ValueTask<RunSummary> RunLoadTestClass(
        LoadTestCollectionRunnerContext ctxt,
        LoadTestClass testClass,
        LoadTestCase[] testCases)
    {
        object? testClassInstance = null;

        // We don't use the aggregator here because we're shortcutting everything to just return failure,
        // so the exception will already be reported and doesn't need to propagated.
        try
        {
            testClassInstance = Activator.CreateInstance(testClass.Class);
        }
        catch (Exception ex)
        {
            return await FailTestClass(ctxt, testClass, testCases, ex);
        }

        if (testClassInstance is not Specification specification)
            return await FailTestClass(ctxt, testClass, testCases, new TestPipelineException($"Test class {testClass.Class.FullName} cannot be static, and must derive from Specification."));

        try
        {
            specification.OnStart();
        }
        catch (Exception ex)
        {
            return await FailTestClass(ctxt, testClass, testCases, ex);
        }

        var result = await LoadTestClassRunner.Instance.Run(specification, testClass, testCases, ctxt.MessageBus, ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource);

        ctxt.Aggregator.Run(specification.OnFinish);

        if (specification is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (specification is IDisposable disposable)
            disposable.Dispose();

        return result;
    }

    private async ValueTask<RunSummary> RunStandardTestCases(
        LoadTestCollectionRunnerContext ctxt,
        LoadTestClass testClass,
        object testClassInstance,
        StandardTestCase[] testCases)
    {
        var summary = new RunSummary();

        foreach (var testCase in testCases)
        {
            try
            {
                var result = await RunSingleStandardTest(ctxt, testCase, testClassInstance);
                summary.Aggregate(result);
            }
            catch (Exception ex)
            {
                // Handle individual test failure
                var failedResult = XunitRunnerHelper.FailTestCases(
                    ctxt.MessageBus,
                    ctxt.CancellationTokenSource,
                    new[] { testCase },
                    ex,
                    sendTestClassMessages: false,
                    sendTestMethodMessages: true
                );
                summary.Aggregate(failedResult);
            }
        }

        return summary;
    }

    private async ValueTask<RunSummary> RunSingleStandardTest(
        LoadTestCollectionRunnerContext ctxt,
        StandardTestCase testCase,
        object testClassInstance)
    {
        var testMethod = (LoadTestMethod)testCase.TestMethod;
        var method = testMethod.Method;

        var summary = new RunSummary();

        // Support [Theory]-style data driven tests
        var dataAttributes = method.GetCustomAttributes(typeof(DataAttribute), false)
            .Cast<DataAttribute>()
            .ToArray();

        if (dataAttributes.Length > 0)
        {
            foreach (var dataAttribute in dataAttributes)
            {
                foreach (var dataRow in dataAttribute.GetData(method))
                {
                    summary.Total++;
                    try
                    {
                        var result = method.Invoke(testClassInstance, dataRow);
                        if (result is Task task)
                            await task;
                    }
                    catch (Exception)
                    {
                        summary.Failed++;
                    }
                }
            }

            return summary;
        }

        // No data attributes - treat as standard [Fact]
        try
        {
            var result = method.Invoke(testClassInstance, null);
            if (result is Task task)
                await task;
            summary.Total = 1;
        }
        catch (Exception)
        {
            summary.Total = 1;
            summary.Failed = 1;
        }

        return summary;
    }

    public async ValueTask<RunSummary> Run(
        LoadCollection testCollection,
        IReadOnlyCollection<LoadTestCase> testCases,
        IMessageBus messageBus,
        ExceptionAggregator exceptionAggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        await using var ctxt = new LoadTestCollectionRunnerContext(testCollection, testCases, messageBus, exceptionAggregator, cancellationTokenSource);
        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }
}

public class LoadTestCollectionRunnerContext(
    LoadCollection testCollection,
    IReadOnlyCollection<LoadTestCase> testCases,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) :
        TestCollectionRunnerContext<LoadCollection, LoadTestCase>(testCollection, testCases, ExplicitOption.Off, messageBus, aggregator, cancellationTokenSource)
{ }
