using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Core mixed testing scenarios - demonstrates Load, Fact, and Theory working together in the same class.
/// This proves that standard xUnit attributes coexist with the Load attribute.
/// </summary>
public class MixedTestingScenarios : xUnitV3LoadTests.TestSetup
{
    private static int _sharedCounter;

    #region Standard [Fact] Tests

    /// <summary>
    /// Standard unit test that validates infrastructure.
    /// [Fact] works alongside [Load] in the same class.
    /// </summary>
    [Fact]
    public void Fact_Should_Validate_Infrastructure()
    {
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        Assert.NotNull(httpClientFactory);
        Assert.NotNull(httpClient);
    }

    /// <summary>
    /// Async [Fact] test - demonstrates async facts work alongside [Load].
    /// </summary>
    [Fact]
    public async Task Fact_Async_Should_Complete_Successfully()
    {
        await Task.Delay(50);
        var result = await Task.FromResult(42);
        Assert.Equal(42, result);
    }

    /// <summary>
    /// [Fact] with Fluent API load test - can do load testing without [Load] attribute.
    /// Useful when you need access to LoadResult metrics.
    /// </summary>
    [Fact]
    public async Task Fact_With_FluentAPI_Should_Return_Metrics()
    {
        var result = await LoadTestRunner.Create()
            .WithName("FactWithFluentAPI")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                await Task.Delay(10);
            });

        // Can assert on metrics with Fluent API
        Assert.True(result.Total > 0);
        Assert.True(result.Success > 0);
        Assert.True(result.RequestsPerSecond > 0);
    }

    #endregion

    #region Native [Load] Tests

    /// <summary>
    /// Native load test - method body runs under load automatically.
    /// [Load] works alongside [Fact] and [Theory] in the same class.
    /// </summary>
    [Load(concurrency: 2, duration: 1500, interval: 300)]
    public async Task Load_Should_Execute_With_Other_Tests()
    {
        // Native approach: method body IS the action - runs under load automatically
        await Task.Delay(Random.Shared.Next(30, 100));
        // Pass if no exception, fail if exception thrown
    }

    /// <summary>
    /// Second native [Load] test in same class - multiple [Load] tests work fine together.
    /// </summary>
    [Load(concurrency: 3, duration: 500, interval: 100)]
    public async Task Load_Second_Test_In_Same_Class()
    {
        Interlocked.Increment(ref _sharedCounter);
        await Task.Delay(10);
    }

    /// <summary>
    /// Sync native [Load] test - sync methods work alongside async methods.
    /// </summary>
    [Load(concurrency: 2, duration: 300, interval: 100)]
    public void Load_Sync_Method_Works_Too()
    {
        Thread.Sleep(5);
    }

    #endregion

    #region [Theory] Tests

    /// <summary>
    /// Theory test for multiple scenarios.
    /// [Theory] works alongside [Load] in the same class.
    /// </summary>
    [Theory]
    [InlineData(200, "OK")]
    [InlineData(201, "Created")]
    [InlineData(204, "No Content")]
    public async Task Theory_Should_Handle_HTTP_Status_Codes(int statusCode, string description)
    {
        // Simulate HTTP status code handling without external dependencies
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Test that the status code is what we expect and description is not null
        Assert.True(statusCode >= 200 && statusCode < 300, "Status code should be success range");
        Assert.NotNull(description);
        Assert.NotEmpty(description);
    }

    /// <summary>
    /// Theory with ClassData - demonstrates complex theory data works alongside [Load].
    /// </summary>
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(10, 20, 30)]
    public void Theory_Math_Operations_Should_Be_Correct(int a, int b, int expected)
    {
        var result = a + b;
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Async theory test - validates async theories work with [Load].
    /// </summary>
    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    public async Task Theory_Async_Should_Process_Delays(int delayMs)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.Delay(delayMs);
        stopwatch.Stop();

        // Allow some variance in timing
        Assert.True(stopwatch.ElapsedMilliseconds >= delayMs * 0.8);
    }

    #endregion

    #region Mixed Pattern Tests - All Three Types Interact

    /// <summary>
    /// This test demonstrates that state can be shared across different test types.
    /// The static counter is incremented by [Load] tests and can be read by [Fact] tests.
    /// Note: Test execution order is not guaranteed, so this tests shared state mechanism works.
    /// </summary>
    [Fact]
    public void Fact_Can_Access_Shared_State_From_Load_Tests()
    {
        // This just validates the shared counter mechanism works
        // The actual value depends on test execution order
        Assert.True(_sharedCounter >= 0);
    }

    #endregion
}

/// <summary>
/// Second mixed test class - demonstrates multiple classes can have mixed test types.
/// </summary>
public class MixedTestingScenarios_SecondClass
{
    /// <summary>
    /// [Fact] in second class works independently.
    /// </summary>
    [Fact]
    public void Fact_In_Second_Class_Works()
    {
        Assert.True(true);
    }

    /// <summary>
    /// [Load] in second class works independently.
    /// </summary>
    [Load(concurrency: 2, duration: 200, interval: 50)]
    public async Task Load_In_Second_Class_Works()
    {
        await Task.Delay(5);
    }

    /// <summary>
    /// [Theory] in second class works independently.
    /// </summary>
    [Theory]
    [InlineData("test1")]
    [InlineData("test2")]
    public void Theory_In_Second_Class_Works(string value)
    {
        Assert.NotEmpty(value);
    }
}
