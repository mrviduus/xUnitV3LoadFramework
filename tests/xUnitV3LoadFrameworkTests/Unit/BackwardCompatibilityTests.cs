using System;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFrameworkTests.Unit
{
    /// <summary>
    /// Tests to ensure backward compatibility when upgrading to new graceful stop features.
    /// These tests verify that existing code continues to work without modifications.
    /// </summary>
    public class BackwardCompatibilityTests : IDisposable
    {
        [Fact]
        public async Task Existing_Tests_Should_Work_Without_Changes()
        {
            // Arrange - Simulate existing test code that doesn't use new fields
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Backward_Compatibility_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5
                    // GracefulStopTimeout not specified - should use defaults
                    // TerminationMode not specified - should use Duration (current behavior)
                },
                Action = async () =>
                {
                    await Task.Delay(10);
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert - Should work exactly as before
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
            Assert.Equal(0, result.Failure);
            Assert.True(result.Success > 0);
        }

        [Fact]
        public void Default_Graceful_Stop_Timeout_Should_Be_Reasonable()
        {
            // Arrange & Act
            var shortTest = new LoadSettings { Duration = TimeSpan.FromSeconds(5) };
            var mediumTest = new LoadSettings { Duration = TimeSpan.FromSeconds(30) };
            var longTest = new LoadSettings { Duration = TimeSpan.FromMinutes(10) };

            // Assert - Default timeouts should follow industry standards
            Assert.Equal(TimeSpan.FromSeconds(5), shortTest.EffectiveGracefulStopTimeout); // Min bound
            Assert.Equal(TimeSpan.FromSeconds(9), mediumTest.EffectiveGracefulStopTimeout); // 30% of 30s
            Assert.Equal(TimeSpan.FromSeconds(60), longTest.EffectiveGracefulStopTimeout); // Max bound
        }

        [Fact]
        public void Default_Termination_Mode_Should_Be_Duration()
        {
            // Arrange & Act
            var settings = new LoadSettings
            {
                Duration = TimeSpan.FromSeconds(10),
                Interval = TimeSpan.FromSeconds(1),
                Concurrency = 5
            };

            // Assert - Should default to Duration for backward compatibility
            Assert.Equal(TerminationMode.Duration, settings.TerminationMode);
        }

        [Fact]
        public void Graceful_Stop_Timeout_Null_Should_Use_Calculated_Default()
        {
            // Arrange
            var settings = new LoadSettings
            {
                Duration = TimeSpan.FromSeconds(20),
                Interval = TimeSpan.FromSeconds(1),
                Concurrency = 10,
                GracefulStopTimeout = null // Explicitly null
            };

            // Act & Assert
            Assert.Null(settings.GracefulStopTimeout);
            Assert.Equal(TimeSpan.FromSeconds(6), settings.EffectiveGracefulStopTimeout); // 30% of 20s
        }

        public void Dispose()
        {
            // Clean up any resources if needed
        }
    }
}
