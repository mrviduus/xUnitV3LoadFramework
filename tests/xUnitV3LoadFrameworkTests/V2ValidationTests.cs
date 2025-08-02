using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFrameworkTests.V2Tests;

/// <summary>
/// Test to validate the new v2 StressAttribute functionality
/// </summary>
[UseStressFramework]
public class StressAttributeValidationTests
{
    [Fact]
    public void Standard_Unit_Test_Should_Work()
    {
        // Standard xUnit test to verify mixed test support
        Assert.True(true);
        Console.WriteLine("Standard unit test executed successfully");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Standard_Theory_Test_Should_Work(int value)
    {
        // Standard xUnit theory test
        Assert.True(value > 0);
        Console.WriteLine($"Theory test executed with value: {value}");
    }

    [Stress(order: 1, concurrency: 2, duration: 1000, interval: 200)]
    public async Task Stress_Test_Should_Execute()
    {
        // Basic stress test to validate new attribute
        await Task.Delay(10); // Simulate some work
        Assert.True(true);
        Console.WriteLine($"Stress test executed at {DateTime.Now:HH:mm:ss.fff}");
    }

    [Stress(order: 2, concurrency: 3, duration: 1500, interval: 300)]
    public async Task Another_Stress_Test_Should_Execute()
    {
        // Another stress test with different parameters
        await Task.Delay(5); // Simulate light work
        Assert.True(true);
        Console.WriteLine($"Another stress test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}

/// <summary>
/// Test V2 stress framework functionality
/// </summary>
[UseStressFramework]
public class V2StressFrameworkTests : IDisposable
{
    private readonly string _testData;

    public V2StressFrameworkTests()
    {
        _testData = "V2 test data initialized";
        Console.WriteLine("V2 Constructor called - stress framework ready");
    }

    [Stress(order: 1, concurrency: 1, duration: 500, interval: 100)]
    public async Task V2_Stress_Test_Should_Work()
    {
        Assert.NotNull(_testData);
        await Task.Delay(10); // Simulate async work
        Assert.True(true);
        Console.WriteLine("V2 stress test executed successfully");
    }

    [Stress(order: 2, concurrency: 2, duration: 800, interval: 200)]
    public async Task V2_Stress_Test_With_Higher_Concurrency()
    {
        Assert.NotNull(_testData);
        
        var startTime = DateTime.UtcNow;
        await Task.Delay(Random.Shared.Next(5, 25));
        var duration = DateTime.UtcNow - startTime;
        
        Assert.True(duration.TotalMilliseconds >= 5);
        Console.WriteLine($"V2 high concurrency test - Duration: {duration.TotalMilliseconds:F1}ms");
    }

    public void Dispose()
    {
        Console.WriteLine("V2 Dispose called - cleanup completed");
    }
}
