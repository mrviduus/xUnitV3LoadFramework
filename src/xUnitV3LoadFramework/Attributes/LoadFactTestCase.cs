using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Represents a test case for load testing with xUnit v3.
/// Handles the execution of load tests with specified concurrency, duration, and intervals.
/// </summary>
public class LoadFactTestCase : XunitTestCase
{
    private int _order;
    private int _concurrency;
    private int _duration;
    private int _interval;

    /// <summary>
    /// Initializes a new instance of the LoadFactTestCase class.
    /// This constructor is required for deserialization.
    /// </summary>
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public LoadFactTestCase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LoadFactTestCase class with load testing parameters.
    /// </summary>
    /// <param name="testMethod">The test method to execute</param>
    /// <param name="order">Execution order for the test</param>
    /// <param name="concurrency">Number of concurrent executions</param>
    /// <param name="duration">Duration of the load test in milliseconds</param>
    /// <param name="interval">Interval between batches in milliseconds</param>
    public LoadFactTestCase(
        _ITestMethod testMethod,
        int order,
        int concurrency,
        int duration,
        int interval)
        : base(testMethod, [])
    {
        _order = order;
        _concurrency = concurrency;
        _duration = duration;
        _interval = interval;
    }

    /// <summary>
    /// Gets the execution order for this test case.
    /// </summary>
    public int Order => _order;

    /// <summary>
    /// Gets the concurrency level for this test case.
    /// </summary>
    public int Concurrency => _concurrency;

    /// <summary>
    /// Gets the duration for this test case.
    /// </summary>
    public int Duration => _duration;

    /// <summary>
    /// Gets the interval for this test case.
    /// </summary>
    public int Interval => _interval;

    /// <summary>
    /// Creates a test runner for executing this load test case.
    /// </summary>
    /// <returns>A test case runner capable of executing load tests</returns>
    protected override XunitTestCaseRunner CreateTestCaseRunner() =>
        new LoadFactTestCaseRunner(this);

    /// <summary>
    /// Serializes the test case data for cross-process communication.
    /// </summary>
    /// <param name="info">Serialization info to populate</param>
    public override void Serialize(IXunitSerializationInfo info)
    {
        base.Serialize(info);
        
        info.AddValue("Order", _order);
        info.AddValue("Concurrency", _concurrency);
        info.AddValue("Duration", _duration);
        info.AddValue("Interval", _interval);
    }

    /// <summary>
    /// Deserializes the test case data from cross-process communication.
    /// </summary>
    /// <param name="info">Serialization info to read from</param>
    public override void Deserialize(IXunitSerializationInfo info)
    {
        base.Deserialize(info);
        
        _order = info.GetValue<int>("Order");
        _concurrency = info.GetValue<int>("Concurrency");
        _duration = info.GetValue<int>("Duration");
        _interval = info.GetValue<int>("Interval");
    }
}

/// <summary>
/// Test case runner specialized for executing load tests.
/// Integrates with the existing LoadRunner infrastructure.
/// </summary>
public class LoadFactTestCaseRunner : XunitTestCaseRunner
{
    private readonly LoadFactTestCase _loadTestCase;

    /// <summary>
    /// Initializes a new instance of the LoadFactTestCaseRunner class.
    /// </summary>
    /// <param name="testCase">The load test case to run</param>
    public LoadFactTestCaseRunner(LoadFactTestCase testCase)
        : base(testCase)
    {
        _loadTestCase = testCase;
    }

    /// <summary>
    /// Runs the load test case using the LoadRunner infrastructure.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Summary of test execution results</returns>
    public override async ValueTask<RunSummary> RunAsync(CancellationToken cancellationToken = default)
    {
        var summary = new RunSummary { Total = 1 };

        try
        {
            // Create the test class instance
            var testClassInstance = Activator.CreateInstance(TestCase.TestMethod.TestClass.Class);
            
            if (testClassInstance == null)
            {
                summary.Failed = 1;
                return summary;
            }

            // Create load execution plan
            var loadSettings = new LoadSettings
            {
                Concurrency = _loadTestCase.Concurrency,
                Duration = TimeSpan.FromMilliseconds(_loadTestCase.Duration),
                Interval = TimeSpan.FromMilliseconds(_loadTestCase.Interval)
            };

            var executionPlan = new LoadExecutionPlan
            {
                Name = TestCase.TestDisplayName,
                Action = async () =>
                {
                    try
                    {
                        // Execute the test method
                        var method = TestCase.TestMethod.Method;
                        var result = method.Invoke(testClassInstance, []);
                        
                        if (result is Task task)
                        {
                            await task;
                        }
                        else if (result is ValueTask valueTask)
                        {
                            await valueTask;
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but don't fail the entire load test
                        Console.WriteLine($"Load test iteration failed: {ex.Message}");
                        return false;
                    }
                },
                Settings = loadSettings
            };

            // Execute the load test
            var loadResult = await LoadRunner.Run(executionPlan);
            
            // Determine if the test passed based on load results
            if (loadResult.TotalExecutions > 0 && loadResult.SuccessfulExecutions > 0)
            {
                summary.Passed = 1;
                Console.WriteLine($"Load test '{TestCase.TestDisplayName}' completed successfully.");
                Console.WriteLine($"Total executions: {loadResult.TotalExecutions}, Successful: {loadResult.SuccessfulExecutions}");
            }
            else
            {
                summary.Failed = 1;
            }

            // Dispose test class instance if it implements IDisposable
            if (testClassInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else if (testClassInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            summary.Failed = 1;
            Console.WriteLine($"Load test '{TestCase.TestDisplayName}' failed with exception: {ex.Message}");
        }

        return summary;
    }
}
