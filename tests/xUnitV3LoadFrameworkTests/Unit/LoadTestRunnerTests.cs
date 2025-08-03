using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using Xunit;
using System.Reflection;

namespace xUnitV3LoadFramework.Tests.Unit;

/// <summary>
/// Unit tests for LoadTestRunner functionality.
/// Tests the runner methods for executing load tests and parameter validation.
/// </summary>
public class LoadTestRunnerTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Return_LoadResult_With_Success_Metrics()
    {
        // Arrange
        bool testActionCalled = false;
        
        // Act - Use a simple test action that we can verify
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            testActionCalled = true;
            await Task.Delay(10); // Small delay to simulate work
            return true;
        }, "TestMethod");

        // Assert
        Assert.True(testActionCalled, "Test action should have been called");
        Assert.NotNull(result);
        Assert.True(result.Total > 0, "Should have total executions");
        Assert.True(result.Success >= 0, "Should have success count");
        Assert.True(result.Time > 0, "Should have recorded execution time");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Exceptions_Gracefully()
    {
        // Arrange & Act
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
#pragma warning disable CS0162 // Unreachable code detected
            return true;
#pragma warning restore CS0162 // Unreachable code detected
        }, "FailingTestMethod");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Total > 0, "Should have attempted executions");
        Assert.True(result.Failure > 0, "Should have recorded failures");
    }

    [Fact]
    public async Task ExecuteAsync_Synchronous_Should_Work_Correctly()
    {
        // Arrange
        bool testActionCalled = false;
        
        // Act
        var result = await LoadTestRunner.ExecuteAsync(() =>
        {
            testActionCalled = true;
            return true;
        }, "SyncTestMethod");

        // Assert
        Assert.True(testActionCalled, "Synchronous test action should have been called");
        Assert.NotNull(result);
        Assert.True(result.Total > 0, "Should have total executions");
    }

    [Fact]
    public async Task ExecuteAsync_Action_Without_Return_Should_Work()
    {
        // Arrange
        bool testActionCalled = false;
        
        // Act
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            testActionCalled = true;
            await Task.Delay(1);
        }, "VoidTestMethod");

        // Assert
        Assert.True(testActionCalled, "Void test action should have been called");
        Assert.NotNull(result);
        Assert.True(result.Total > 0, "Should have total executions");
    }

    [Fact]
    public void GetCallingTestMethod_Should_Be_Private_And_Not_Accessible()
    {
        // Arrange & Act
        var runnerType = typeof(LoadTestRunner);
        var privateMethod = runnerType.GetMethod("GetCallingTestMethod", BindingFlags.NonPublic | BindingFlags.Static);

        // Assert
        Assert.NotNull(privateMethod);
        Assert.True(privateMethod.IsPrivate, "GetCallingTestMethod should be private");
    }
}