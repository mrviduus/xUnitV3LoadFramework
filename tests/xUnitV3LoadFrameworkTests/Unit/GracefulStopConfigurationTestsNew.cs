using System;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;
using xUnitV3LoadFramework.LoadRunnerCore.Configuration;

namespace xUnitV3LoadFrameworkTests.Unit
{
    /// <summary>
    /// Tests for graceful stop timeout configuration functionality.
    /// Validates the new industry-standard graceful stop behavior.
    /// </summary>
    public class GracefulStopConfigurationTests : IDisposable
    {
        [Fact]
        public async Task Should_Use_Custom_GracefulStopTimeout()
        {
            // Arrange
            var customTimeout = TimeSpan.FromSeconds(2);
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Custom_GracefulStop_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 3,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = customTimeout
                },
                Action = async () =>
                {
                    await Task.Delay(800); // Requests that might extend beyond test duration
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var actualDuration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(actualDuration.TotalSeconds >= 2.0); // At least test duration
            Assert.True(actualDuration.TotalSeconds <= 5.0); // Should not exceed duration + timeout + buffer
            Assert.True(result.Total > 0);
        }

        [Theory]
        [InlineData(TerminationMode.Duration)]
        [InlineData(TerminationMode.CompleteCurrentInterval)]
        [InlineData(TerminationMode.StrictDuration)]
        public async Task Should_Respect_TerminationMode(TerminationMode mode)
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = $"TerminationMode_{mode}_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 3,
                    TerminationMode = mode,
                    GracefulStopTimeout = TimeSpan.FromSeconds(1)
                },
                Action = async () =>
                {
                    await Task.Delay(100);
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.True(result.Total > 0);
            Assert.True(result.Success >= 0);
        }

        [Fact]
        public async Task Should_Handle_Fast_Requests_With_Grace_Period()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Fast_Requests_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromMilliseconds(500)
                },
                Action = async () =>
                {
                    await Task.Delay(50); // Very fast requests
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var actualDuration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(actualDuration.TotalSeconds >= 3.0);
            Assert.True(actualDuration.TotalSeconds <= 5.0); // Should complete quickly
            Assert.Equal(15, result.Total); // 3 intervals × 5 requests
        }

        [Fact]
        public async Task Should_Handle_Slow_Requests_With_Grace_Period()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Slow_Requests_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 2,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(3)
                },
                Action = async () =>
                {
                    await Task.Delay(1500); // Slow requests that extend beyond intervals
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var actualDuration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(actualDuration.TotalSeconds >= 2.0);
            Assert.True(actualDuration.TotalSeconds <= 6.0); // Duration + graceful stop + buffer
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task Should_Work_With_Hybrid_Mode()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Hybrid_GracefulStop_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2),
                    UseHybridMode = true
                },
                Action = async () =>
                {
                    await Task.Delay(200);
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var actualDuration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(actualDuration.TotalSeconds >= 3.0);
            Assert.True(actualDuration.TotalSeconds <= 6.0);
            Assert.Equal(15, result.Total); // 3 intervals × 5 requests
        }

        [Fact]
        public async Task Should_Validate_GracefulStopTimeout_Bounds()
        {
            // Test that bounds validation works as expected
            var settings = new LoadSettings
            {
                Duration = TimeSpan.FromSeconds(30),
                Interval = TimeSpan.FromSeconds(1),
                Concurrency = 10
            };

            // Test default calculation (30% of 30 seconds = 9 seconds, within bounds)
            var defaultTimeout = settings.EffectiveGracefulStopTimeout;
            Assert.True(defaultTimeout.TotalSeconds >= 5);
            Assert.True(defaultTimeout.TotalSeconds <= 60);

            // Test with short duration (should default to minimum)
            settings.Duration = TimeSpan.FromSeconds(10);
            var shortTimeout = settings.EffectiveGracefulStopTimeout;
            Assert.Equal(5, shortTimeout.TotalSeconds); // Should be minimum

            // Test with very long duration (should cap at maximum)
            settings.Duration = TimeSpan.FromMinutes(10);
            var longTimeout = settings.EffectiveGracefulStopTimeout;
            Assert.Equal(60, longTimeout.TotalSeconds); // Should be maximum
        }

        public void Dispose()
        {
            // Clean up any resources if needed
        }
    }
}
