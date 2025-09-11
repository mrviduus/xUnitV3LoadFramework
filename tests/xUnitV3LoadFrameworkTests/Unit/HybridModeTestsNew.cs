using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFrameworkTests.Unit
{
    /// <summary>
    /// Tests for the default hybrid mode behavior.
    /// Note: The framework uses hybrid mode by default, so these tests validate hybrid functionality.
    /// </summary>
    public class HybridModeTests : IDisposable
    {
        [Fact]
        public async Task Should_Execute_Exactly_100_Requests_For_10_RPS_10_Seconds()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Hybrid_10_RPS_10_Seconds",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(10),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 10,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(5)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(15); // Slightly longer delay
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(100, result.Total);
            Assert.Equal(100, requestCount);
            Assert.True(result.Success > 0);
            Assert.Equal(0, result.Failure);
        }

        [Fact]
        public async Task Should_Handle_High_Concurrency()
        {
            // Arrange - Test with higher concurrency
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "High_Concurrency",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(5),
                    Interval = TimeSpan.FromMilliseconds(500), // Every 0.5 seconds
                    Concurrency = 20, // 20 requests every 0.5s = 40 RPS
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(3)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(10);
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(200, result.Total); // 10 intervals × 20 requests
            Assert.Equal(200, requestCount);
        }

        [Fact]
        public async Task Should_Complete_Within_Graceful_Stop_Timeout()
        {
            // Arrange - Test with slow requests and custom graceful stop timeout
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Graceful_Stop_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(500); // Slower requests
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var endTime = DateTime.UtcNow;
            var actualDuration = endTime - startTime;

            // Assert
            // Should complete within test duration + graceful stop timeout + small buffer
            Assert.True(actualDuration.TotalSeconds <= 6.0); // 3s + 2s + 1s buffer
            Assert.Equal(15, result.Total); // 3 intervals × 5 requests
            Assert.Equal(15, requestCount);
        }

        [Fact]
        public async Task Should_Handle_Mixed_Success_And_Failure()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Mixed_Results",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(4),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2)
                },
                Action = async () =>
                {
                    var count = Interlocked.Increment(ref requestCount);
                    await Task.Delay(10);
                    
                    // Make every 3rd request fail
                    return count % 3 != 0;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(20, result.Total); // 4 intervals × 5 requests
            Assert.Equal(20, requestCount);
            Assert.True(result.Success > 0);
            Assert.True(result.Failure > 0);
            Assert.Equal(result.Success + result.Failure, result.Total);
        }

        [Fact]
        public async Task Should_Handle_StrictDuration_Termination()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "StrictDuration",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 10,
                    TerminationMode = TerminationMode.StrictDuration,
                    GracefulStopTimeout = TimeSpan.FromSeconds(1)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(100); // Longer requests
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var endTime = DateTime.UtcNow;
            var actualDuration = endTime - startTime;

            // Assert
            // StrictDuration should stop quickly
            Assert.True(actualDuration.TotalSeconds <= 5.0); // Should be close to 3s + minimal overhead
            Assert.True(result.Total <= 30); // May be less than full count due to strict cutoff
            Assert.Equal(result.Total, requestCount);
        }

        [Fact]
        public async Task Should_Handle_Exception_Gracefully()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Exception_Handling",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 3,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(1)
                },
                Action = async () =>
                {
                    var count = Interlocked.Increment(ref requestCount);
                    await Task.Delay(10);
                    
                    // Make every 2nd request throw an exception
                    if (count % 2 == 0)
                    {
                        throw new InvalidOperationException("Test exception");
                    }
                    
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(6, result.Total); // 2 intervals × 3 requests
            Assert.Equal(6, requestCount);
            Assert.True(result.Success > 0);
            Assert.True(result.Failure > 0);
            Assert.Equal(result.Success + result.Failure, result.Total);
        }

        public void Dispose()
        {
            // Clean up any resources if needed
        }
    }
}
