using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFrameworkTests.Integration;

/// <summary>
/// Tests demonstrating native xUnit v3 load test execution.
/// The test method body becomes the action - no manual LoadTestRunner.ExecuteAsync() call needed.
/// Includes success scenarios, failure scenarios, and edge cases.
/// </summary>
public class NativeLoadTests
{
    private static int _counter;
    private static int _successCounter;

    #region Success Scenarios

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
    /// Returns true = success.
    /// </summary>
    [Load(concurrency: 2, duration: 500, interval: 100)]
    public Task<bool> Native_Load_Test_Returns_Bool_Success()
    {
        // Return true for success
        return Task.FromResult(true);
    }

    /// <summary>
    /// Native load test with ValueTask return type.
    /// </summary>
    [Load(concurrency: 2, duration: 500, interval: 100)]
    public ValueTask Native_Load_Test_ValueTask()
    {
        // ValueTask methods work too
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Native load test with ValueTask<bool> return type.
    /// </summary>
    [Load(concurrency: 2, duration: 500, interval: 100)]
    public ValueTask<bool> Native_Load_Test_ValueTask_Bool()
    {
        return ValueTask.FromResult(true);
    }

    #endregion

    #region Edge Cases - Short Duration and Minimal Configuration

    /// <summary>
    /// Very short duration test - validates framework handles quick tests.
    /// </summary>
    [Load(concurrency: 1, duration: 100, interval: 50)]
    public async Task Native_Load_Test_Very_Short_Duration()
    {
        await Task.Delay(5);
    }

    /// <summary>
    /// Single concurrency test - validates framework works with minimal load.
    /// </summary>
    [Load(concurrency: 1, duration: 300, interval: 100)]
    public async Task Native_Load_Test_Single_Concurrency()
    {
        Interlocked.Increment(ref _counter);
        await Task.Delay(10);
    }

    /// <summary>
    /// High concurrency test - validates framework handles many concurrent operations.
    /// </summary>
    [Load(concurrency: 20, duration: 500, interval: 100)]
    public async Task Native_Load_Test_High_Concurrency()
    {
        await Task.Delay(5);
    }

    /// <summary>
    /// Long interval test - validates framework handles sparse request patterns.
    /// </summary>
    [Load(concurrency: 2, duration: 1000, interval: 500)]
    public async Task Native_Load_Test_Long_Interval()
    {
        await Task.Delay(10);
    }

    #endregion

    #region Thread Safety Tests

    /// <summary>
    /// Test with shared mutable state - validates thread-safe operations.
    /// Uses Interlocked to ensure thread safety.
    /// </summary>
    [Load(concurrency: 10, duration: 500, interval: 50)]
    public async Task Native_Load_Test_Thread_Safe_Counter()
    {
        // Thread-safe increment
        var value = Interlocked.Increment(ref _successCounter);
        await Task.Delay(5);

        // Validate counter is positive (basic sanity check)
        if (value <= 0)
        {
            throw new InvalidOperationException("Counter should be positive");
        }
    }

    /// <summary>
    /// Test with async/await context switching.
    /// Validates framework handles context switches correctly.
    /// </summary>
    [Load(concurrency: 5, duration: 500, interval: 100)]
    public async Task Native_Load_Test_Context_Switching()
    {
        // Multiple awaits cause context switches
        await Task.Delay(5);
        var result = await Task.Run(() => 42);
        await Task.Delay(5);

        if (result != 42)
        {
            throw new InvalidOperationException("Context switch corrupted data");
        }
    }

    #endregion
}
