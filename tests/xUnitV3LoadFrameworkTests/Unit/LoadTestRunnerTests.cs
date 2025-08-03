using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.Attributes;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Unit;

/// <summary>
/// Core unit tests for LoadTestRunner functionality.
/// Tests the essential runner methods for executing load tests.
/// </summary>
public class LoadTestRunnerTests
{
    [Load(order: 1, concurrency: 2, duration: 1000, interval: 200)]
    public async Task LoadTestRunner_Should_Execute_With_Load_Attribute()
    {
        // Arrange
        bool testActionCalled = false;
        
        // Act - This will work because it's called from a method with Load attribute
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            testActionCalled = true;
            await Task.Delay(10); // Small delay to simulate work
            return true;
        });

        // Assert
        Assert.True(testActionCalled, "Test action should have been called");
        Assert.NotNull(result);
        Assert.True(result.Total > 0, "Should have total executions");
        Assert.True(result.Success >= 0, "Should have success count");
        Assert.True(result.Time > 0, "Should have recorded execution time");
    }
}