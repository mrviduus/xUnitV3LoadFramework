using System;
using System.Threading.Tasks;
using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;
using Xunit;

namespace LoadRunnerTests;

public class LoadRunnerTests
{
    [Fact]
    public async Task Run_ReturnsLoadResult_WhenExecutionPlanIsValid()
    {
        // Arrange
        var executionPlan = new LoadExecutionPlan
        {
            Name = "TestPlan",
            Action = () => Task.FromResult(true),
            Settings = new LoadSettings { Duration = TimeSpan.FromSeconds(10), Concurrency = 1 }
        };

        // Act
        var result = await LoadRunner.Run(executionPlan);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestPlan", result.ScenarioName);
    }

    [Fact]
    public async Task Run_ThrowsArgumentNullException_WhenActionIsNull()
    {
        // Arrange
        var executionPlan = new LoadExecutionPlan
        {
            Name = "TestPlan",
            Action = null,
            Settings = new LoadSettings { Duration = TimeSpan.FromSeconds(10), Concurrency = 1 }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => LoadRunner.Run(executionPlan));
    }

    [Fact]
    public async Task Run_ReturnsLoadResult_WhenConcurrencyIsZero()
    {
        // Arrange
        var executionPlan = new LoadExecutionPlan
        {
            Name = "TestPlan",
            Action = () => Task.FromResult(true),
            Settings = new LoadSettings { Duration = TimeSpan.FromSeconds(10), Concurrency = 0 }
        };

        // Act
        var result = await LoadRunner.Run(executionPlan);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestPlan", result.ScenarioName);
    }
}