using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFrameworkTests.Integration;

/// <summary>
/// Tests demonstrating native xUnit v3 load test execution.
/// The test method body becomes the action - no manual LoadTestRunner.ExecuteAsync() call needed.
/// </summary>
public class NativeLoadTests
{
    private static int _counter;

    /// <summary>
    /// Native load test - method body runs N times under load automatically.
    /// No need to call LoadTestRunner.ExecuteAsync().
    /// </summary>
    [Load(concurrency: 3, duration: 1000, interval: 200)]
    public async Task Native_Load_Test_Without_Manual_ExecuteAsync()
    {
        // This entire method body runs as the action under load
        Interlocked.Increment(ref _counter);
        await Task.Delay(10);
    }

    /// <summary>
    /// Native load test with sync method body.
    /// </summary>
    [Load(concurrency: 2, duration: 500, interval: 100)]
    public void Native_Sync_Load_Test()
    {
        // Sync methods work too
        Thread.Sleep(5);
    }

    /// <summary>
    /// Native load test that returns bool to indicate success/failure.
    /// </summary>
    [Load(concurrency: 2, duration: 500, interval: 100)]
    public Task<bool> Native_Load_Test_Returns_Bool()
    {
        // Return true for success, false for failure
        return Task.FromResult(true);
    }
}
