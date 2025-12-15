using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoadSurge.Models;
using LoadSurge.Runner;
using Xunit.Sdk;
using Xunit.v3;

namespace xUnitV3LoadFramework.Discovery;

/// <summary>
/// Custom test case for load tests. Implements <see cref="ISelfExecutingXunitTestCase"/>
/// to execute the test method as a load test action.
/// </summary>
public class LoadTestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
    /// <summary>
    /// Gets the number of concurrent executions per batch.
    /// </summary>
    public int Concurrency { get; private set; }

    /// <summary>
    /// Gets the duration of the load test in milliseconds.
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Gets the interval between batches in milliseconds.
    /// </summary>
    public int Interval { get; private set; }

    /// <summary>
    /// Called by deserializer; should only be called by deriving classes for deserialization purposes.
    /// </summary>
    [Obsolete("For deserialization only")]
    public LoadTestCase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadTestCase"/> class.
    /// </summary>
    public LoadTestCase(
        IXunitTestMethod testMethod,
        int concurrency,
        int duration,
        int interval,
        string? displayName,
        string? skipReason,
        bool @explicit,
        int timeout)
        : base(
            testMethod: testMethod,
            testCaseDisplayName: $"{displayName ?? testMethod.MethodName} [Load({concurrency}, {duration}ms, {interval}ms)]",
            uniqueID: GenerateUniqueID(testMethod, concurrency, duration, interval),
            @explicit: @explicit,
            skipReason: skipReason,
            skipType: null,
            skipUnless: null,
            skipWhen: null,
            traits: null,
            testMethodArguments: null,
            sourceFilePath: null,
            sourceLineNumber: null,
            timeout: timeout > 0 ? (int?)timeout : null)
    {
        Concurrency = concurrency;
        Duration = duration;
        Interval = interval;
    }

    /// <summary>
    /// Executes the load test by invoking the test method as the action delegate.
    /// </summary>
    public async ValueTask<RunSummary> Run(
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var summary = new RunSummary { Total = 1 };

        // Check for skip
        if (!string.IsNullOrEmpty(SkipReason))
        {
            summary.Skipped = 1;
            var skipSummary = XunitRunnerHelper.SkipTestCases(
                messageBus,
                cancellationTokenSource,
                new IXunitTestCase[] { this },
                SkipReason,
                sendTestCollectionMessages: false,
                sendTestClassMessages: false,
                sendTestMethodMessages: false,
                sendTestCaseMessages: true,
                sendTestMessages: true);
            return skipSummary;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        object? testClassInstance = null;
        LoadResult? loadResult = null;
        Exception? testException = null;
        var output = new StringBuilder();

        try
        {
            // Create test class instance
            testClassInstance = CreateTestClassInstance(constructorArguments);

            // Create action delegate from test method
            var action = CreateActionFromTestMethod(testClassInstance);

            // Build and execute load test
            var plan = new LoadExecutionPlan
            {
                Name = TestCaseDisplayName,
                Settings = new LoadSettings
                {
                    Concurrency = Concurrency,
                    Duration = TimeSpan.FromMilliseconds(Duration),
                    Interval = TimeSpan.FromMilliseconds(Interval)
                },
                Action = action
            };

            loadResult = await LoadRunner.Run(plan);

            // Build output
            BuildOutput(output, loadResult);
        }
        catch (Exception ex)
        {
            testException = ex.InnerException ?? ex;
            output.AppendLine($"Load test failed with exception: {testException.Message}");
        }
        finally
        {
            stopwatch.Stop();

            // Dispose test class if needed
            if (testClassInstance is IAsyncDisposable asyncDisposable)
            {
                await aggregator.RunAsync(asyncDisposable.DisposeAsync);
            }
            else if (testClassInstance is IDisposable disposable)
            {
                aggregator.Run(disposable.Dispose);
            }
        }

        var executionTime = (decimal)stopwatch.Elapsed.TotalSeconds;
        summary.Time = executionTime;

        // Create test for messaging
        var test = CreateTest();
        var finishTime = DateTimeOffset.Now;
        var startTime = finishTime.AddSeconds(-(double)executionTime);
        var outputString = output.ToString();

        // Convert traits to the expected format
        var traitsDict = Traits.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyCollection<string>)kvp.Value.ToList());

        // Send TestCaseStarting
        messageBus.QueueMessage(new TestCaseStarting
        {
            AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
            TestCaseUniqueID = UniqueID,
            TestClassUniqueID = TestMethod.TestClass.UniqueID,
            TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
            TestMethodUniqueID = TestMethod.UniqueID,
            TestCaseDisplayName = TestCaseDisplayName,
            SkipReason = SkipReason,
            SourceFilePath = SourceFilePath,
            SourceLineNumber = SourceLineNumber,
            Traits = traitsDict,
            Explicit = Explicit,
            TestClassMetadataToken = TestMethod.TestClass.Class.MetadataToken,
            TestClassName = TestMethod.TestClass.Class.FullName ?? TestMethod.TestClass.Class.Name,
            TestClassNamespace = TestMethod.TestClass.Class.Namespace,
            TestClassSimpleName = TestMethod.TestClass.Class.Name,
            TestMethodMetadataToken = TestMethod.Method.MetadataToken,
            TestMethodName = TestMethod.Method.Name,
            TestMethodArity = TestMethod.Method.IsGenericMethodDefinition ? TestMethod.Method.GetGenericArguments().Length : 0,
            TestMethodParameterTypesVSTest = TestMethod.Method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name).ToArray(),
            TestMethodReturnTypeVSTest = TestMethod.Method.ReturnType.FullName ?? TestMethod.Method.ReturnType.Name
        });

        // Send TestStarting
        messageBus.QueueMessage(new TestStarting
        {
            AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
            TestCaseUniqueID = UniqueID,
            TestClassUniqueID = TestMethod.TestClass.UniqueID,
            TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
            TestMethodUniqueID = TestMethod.UniqueID,
            TestUniqueID = test.UniqueID,
            TestDisplayName = TestCaseDisplayName,
            Explicit = Explicit,
            StartTime = startTime,
            Timeout = Timeout,
            Traits = traitsDict
        });

        // Determine pass/fail and send result
        if (testException != null)
        {
            summary.Failed = 1;
            messageBus.QueueMessage(new TestFailed
            {
                AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
                TestCaseUniqueID = UniqueID,
                TestClassUniqueID = TestMethod.TestClass.UniqueID,
                TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
                TestMethodUniqueID = TestMethod.UniqueID,
                TestUniqueID = test.UniqueID,
                ExecutionTime = executionTime,
                FinishTime = finishTime,
                Output = outputString,
                Warnings = Array.Empty<string>(),
                Cause = FailureCause.Exception,
                ExceptionParentIndices = new int[] { -1 },
                ExceptionTypes = new string[] { testException.GetType().FullName ?? testException.GetType().Name },
                Messages = new string[] { testException.Message },
                StackTraces = new string?[] { testException.StackTrace ?? "" }
            });
        }
        else if (loadResult != null && loadResult.Failure > 0)
        {
            summary.Failed = 1;
            var failureMessage = $"Load test had {loadResult.Failure} failures out of {loadResult.Total} total executions ({(double)loadResult.Failure / loadResult.Total * 100:F1}% failure rate)";

            messageBus.QueueMessage(new TestFailed
            {
                AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
                TestCaseUniqueID = UniqueID,
                TestClassUniqueID = TestMethod.TestClass.UniqueID,
                TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
                TestMethodUniqueID = TestMethod.UniqueID,
                TestUniqueID = test.UniqueID,
                ExecutionTime = executionTime,
                FinishTime = finishTime,
                Output = outputString,
                Warnings = Array.Empty<string>(),
                Cause = FailureCause.Assertion,
                ExceptionParentIndices = new int[] { -1 },
                ExceptionTypes = new string[] { "Xunit.Sdk.XunitException" },
                Messages = new string[] { failureMessage },
                StackTraces = new string?[] { "" }
            });
        }
        else
        {
            messageBus.QueueMessage(new TestPassed
            {
                AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
                TestCaseUniqueID = UniqueID,
                TestClassUniqueID = TestMethod.TestClass.UniqueID,
                TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
                TestMethodUniqueID = TestMethod.UniqueID,
                TestUniqueID = test.UniqueID,
                ExecutionTime = executionTime,
                FinishTime = finishTime,
                Output = outputString,
                Warnings = Array.Empty<string>()
            });
        }

        // Send TestFinished
        messageBus.QueueMessage(new TestFinished
        {
            AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
            TestCaseUniqueID = UniqueID,
            TestClassUniqueID = TestMethod.TestClass.UniqueID,
            TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
            TestMethodUniqueID = TestMethod.UniqueID,
            TestUniqueID = test.UniqueID,
            ExecutionTime = executionTime,
            FinishTime = finishTime,
            Output = outputString,
            Warnings = Array.Empty<string>(),
            Attachments = new Dictionary<string, TestAttachment>()
        });

        // Send TestCaseFinished
        messageBus.QueueMessage(new TestCaseFinished
        {
            AssemblyUniqueID = TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
            TestCaseUniqueID = UniqueID,
            TestClassUniqueID = TestMethod.TestClass.UniqueID,
            TestCollectionUniqueID = TestMethod.TestClass.TestCollection.UniqueID,
            TestMethodUniqueID = TestMethod.UniqueID,
            ExecutionTime = executionTime,
            TestsFailed = summary.Failed,
            TestsNotRun = 0,
            TestsSkipped = summary.Skipped,
            TestsTotal = summary.Total
        });

        return summary;
    }

    private XunitTest CreateTest()
    {
        // Convert traits format
        var traitsDict = Traits.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyCollection<string>)kvp.Value.ToList());

        return new XunitTest(
            this,
            TestMethod,
            @explicit: Explicit ? true : null,
            skipReason: SkipReason,
            skipType: SkipType,
            skipUnless: SkipUnless,
            skipWhen: SkipWhen,
            testDisplayName: TestCaseDisplayName,
            testIndex: 0,
            traits: traitsDict,
            timeout: Timeout,
            testMethodArguments: Array.Empty<object?>());
    }

    private object CreateTestClassInstance(object?[] constructorArguments)
    {
        var testClassType = TestMethod.TestClass.Class;
        return Activator.CreateInstance(testClassType, constructorArguments)
            ?? throw new InvalidOperationException($"Failed to create instance of {testClassType.FullName}");
    }

    private Func<Task<bool>> CreateActionFromTestMethod(object testClassInstance)
    {
        var method = TestMethod.Method;

        return async () =>
        {
            try
            {
                var result = method.Invoke(testClassInstance, null);

                if (result is Task<bool> taskBool)
                {
                    return await taskBool;
                }
                else if (result is Task task)
                {
                    await task;
                    return true;
                }
                else if (result is ValueTask<bool> valueTaskBool)
                {
                    return await valueTaskBool;
                }
                else if (result is ValueTask valueTask)
                {
                    await valueTask;
                    return true;
                }
                else if (result is bool boolResult)
                {
                    return boolResult;
                }

                return true;
            }
            catch
            {
                return false;
            }
        };
    }

    private static void BuildOutput(StringBuilder sb, LoadResult? result)
    {
        if (result == null)
        {
            sb.AppendLine("Load test completed but no results were returned.");
            return;
        }

        var successRate = result.Total > 0 ? (double)result.Success / result.Total * 100 : 0;
        var status = result.Failure == 0 ? "PASSED" : "FAILED";

        sb.AppendLine($"Load Test Results:");
        sb.AppendLine($"  Total: {result.Total}, Success: {result.Success}, Failure: {result.Failure}");
        sb.AppendLine($"  RPS: {result.RequestsPerSecond:F1}, Avg: {result.AverageLatency:F0}ms, P95: {result.Percentile95Latency:F0}ms, P99: {result.Percentile99Latency:F0}ms");
        sb.AppendLine($"  Result: {status} ({successRate:F1}% success rate)");
    }

    private static string GenerateUniqueID(IXunitTestMethod testMethod, int concurrency, int duration, int interval)
    {
        var className = testMethod.TestClass.Class.FullName ?? testMethod.TestClass.Class.Name;
        var methodName = testMethod.Method.Name;
        return $"{className}.{methodName}:Load({concurrency},{duration},{interval})";
    }

    /// <inheritdoc/>
    protected override void Serialize(IXunitSerializationInfo info)
    {
        base.Serialize(info);
        info.AddValue(nameof(Concurrency), Concurrency);
        info.AddValue(nameof(Duration), Duration);
        info.AddValue(nameof(Interval), Interval);
    }

    /// <inheritdoc/>
    protected override void Deserialize(IXunitSerializationInfo info)
    {
        base.Deserialize(info);
        Concurrency = info.GetValue<int>(nameof(Concurrency));
        Duration = info.GetValue<int>(nameof(Duration));
        Interval = info.GetValue<int>(nameof(Interval));
    }
}
