using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Tests that verify the framework correctly detects and reports failures.
/// Uses Fluent API ([Fact] + LoadTestRunner.Create()) to assert on failure metrics.
/// </summary>
public class FailureScenarioTests
{
    private static int _partialFailureCounter;
    private static int _exceptionCounter;

    #region Return False = Failure Detection

    /// <summary>
    /// Verifies that returning false is correctly counted as a failure.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Detect_Failures_When_Action_Returns_False()
    {
        var result = await LoadTestRunner.Create()
            .WithName("ReturnFalse_FailureDetection")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                return false; // Return false = failure
            });

        // Assert: All iterations should be counted as failures
        Assert.True(result.Total > 0, "Should have executed at least one iteration");
        Assert.Equal(result.Total, result.Failure); // All should be failures
        Assert.Equal(0, result.Success); // No successes
    }

    /// <summary>
    /// Verifies partial failures are correctly tracked (some return true, some return false).
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Track_Partial_Failures_Correctly()
    {
        Interlocked.Exchange(ref _partialFailureCounter, 0); // Reset counter

        var result = await LoadTestRunner.Create()
            .WithName("PartialFailures_Detection")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                var iteration = Interlocked.Increment(ref _partialFailureCounter);
                // 50% failure rate
                return iteration % 2 == 0;
            });

        // Assert: Should have both successes and failures
        Assert.True(result.Total > 0, "Should have executed iterations");
        Assert.True(result.Success > 0, "Should have some successes");
        Assert.True(result.Failure > 0, "Should have some failures");
        Assert.Equal(result.Total, result.Success + result.Failure);
    }

    #endregion

    #region Exception = Failure Detection

    /// <summary>
    /// Verifies that exceptions are caught and counted as failures.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Detect_Failures_When_Action_Throws_Exception()
    {
        var result = await LoadTestRunner.Create()
            .WithName("Exception_FailureDetection")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                throw new InvalidOperationException("Simulated failure");
            });

        // Assert: All iterations should be counted as failures due to exceptions
        Assert.True(result.Total > 0, "Should have executed at least one iteration");
        Assert.Equal(result.Total, result.Failure); // All should be failures
        Assert.Equal(0, result.Success); // No successes
    }

    /// <summary>
    /// Verifies intermittent exceptions are correctly tracked.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Track_Intermittent_Exceptions_Correctly()
    {
        Interlocked.Exchange(ref _exceptionCounter, 0); // Reset counter

        var result = await LoadTestRunner.Create()
            .WithName("IntermittentExceptions_Detection")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                var iteration = Interlocked.Increment(ref _exceptionCounter);

                // 33% throw exceptions
                if (iteration % 3 == 0)
                {
                    throw new InvalidOperationException($"Simulated failure at iteration {iteration}");
                }
            });

        // Assert: Should have both successes and failures
        Assert.True(result.Total > 0, "Should have executed iterations");
        Assert.True(result.Success > 0, "Should have some successes");
        Assert.True(result.Failure > 0, "Should have some failures from exceptions");
    }

    /// <summary>
    /// Verifies different exception types are all caught as failures.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Catch_Different_Exception_Types()
    {
        var counter = 0;

        var result = await LoadTestRunner.Create()
            .WithName("DifferentExceptionTypes")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(400))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                var iteration = Interlocked.Increment(ref counter);

                // Throw different exception types
                throw (iteration % 3) switch
                {
                    0 => new InvalidOperationException("InvalidOp"),
                    1 => new ArgumentException("Argument"),
                    _ => new Exception("Generic")
                };
            });

        // Assert: All should be failures regardless of exception type
        Assert.True(result.Total > 0, "Should have executed iterations");
        Assert.Equal(result.Total, result.Failure);
    }

    #endregion

    #region Success Rate Calculations

    /// <summary>
    /// Verifies success rate is calculated correctly with known failure rate.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Calculate_Success_Rate_Correctly()
    {
        var counter = 0;

        var result = await LoadTestRunner.Create()
            .WithName("SuccessRate_Calculation")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                var iteration = Interlocked.Increment(ref counter);
                // 25% failure rate (1 in 4)
                return iteration % 4 != 0;
            });

        // Assert: Success rate should be approximately 75%
        var successRate = (double)result.Success / result.Total;
        Assert.True(result.Total >= 10, "Should have enough iterations for meaningful rate");
        Assert.InRange(successRate, 0.60, 0.90); // Allow some variance
    }

    /// <summary>
    /// Verifies 100% success rate when no failures occur.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Report_100_Percent_Success_When_No_Failures()
    {
        var result = await LoadTestRunner.Create()
            .WithName("PerfectSuccess_Rate")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                // Always succeed
            });

        // Assert: 100% success rate
        Assert.True(result.Total > 0, "Should have executed iterations");
        Assert.Equal(result.Total, result.Success);
        Assert.Equal(0, result.Failure);

        var successRate = (double)result.Success / result.Total;
        Assert.Equal(1.0, successRate);
    }

    /// <summary>
    /// Verifies 0% success rate when all iterations fail.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Report_0_Percent_Success_When_All_Fail()
    {
        var result = await LoadTestRunner.Create()
            .WithName("TotalFailure_Rate")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                return false; // Always fail
            });

        // Assert: 0% success rate
        Assert.True(result.Total > 0, "Should have executed iterations");
        Assert.Equal(0, result.Success);
        Assert.Equal(result.Total, result.Failure);

        var successRate = (double)result.Success / result.Total;
        Assert.Equal(0.0, successRate);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Verifies framework handles async void-like scenarios (Task without return).
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Handle_Void_Task_Success()
    {
        var result = await LoadTestRunner.Create()
            .WithName("VoidTask_Success")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(200))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                // No return value - success if no exception
            });

        Assert.True(result.Total > 0);
        Assert.Equal(result.Total, result.Success);
    }

    /// <summary>
    /// Verifies framework handles null reference exceptions.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Handle_NullReferenceException()
    {
        string? nullString = null;

        var result = await LoadTestRunner.Create()
            .WithName("NullRef_Exception")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(200))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                // This will throw NullReferenceException
                _ = nullString!.Length;
            });

        // All should fail due to NullReferenceException
        Assert.True(result.Total > 0);
        Assert.Equal(result.Total, result.Failure);
    }

    /// <summary>
    /// Verifies framework handles task cancellation.
    /// </summary>
    [Fact]
    public async Task Fluent_Should_Handle_TaskCanceledException()
    {
        var result = await LoadTestRunner.Create()
            .WithName("TaskCanceled_Exception")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(200))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                using var cts = new CancellationTokenSource();
                cts.Cancel(); // Immediately cancel
                await Task.Delay(100, cts.Token); // Will throw TaskCanceledException
            });

        // All should fail due to TaskCanceledException
        Assert.True(result.Total > 0);
        Assert.Equal(result.Total, result.Failure);
    }

    #endregion
}
