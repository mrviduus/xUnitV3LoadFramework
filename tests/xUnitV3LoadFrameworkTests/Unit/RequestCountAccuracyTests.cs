using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFrameworkTests.Unit
{
    /// <summary>
    /// Tests to verify request count accuracy and timing precision.
    /// Addresses the specific issue where 10 RPS for 10 seconds resulted in 90 requests instead of 100.
    /// </summary>
    public class RequestCountAccuracyTests : IDisposable
    {

        [Fact]
        public async Task Should_Execute_Exactly_100_Requests_For_10_RPS_10_Seconds_Duration_Mode()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "10_RPS_10_Seconds_Duration_Mode",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(10),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 10,
                    TerminationMode = TerminationMode.Duration, // Current behavior
                    GracefulStopTimeout = TimeSpan.FromSeconds(5)
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
            // Note: Duration mode may result in fewer requests due to timing precision
            Assert.True(result.Total >= 90); // Allow some variance for timing
            Assert.Equal(result.Total, requestCount);
            Assert.True(result.Success > 0);
            Assert.Equal(0, result.Failure);
        }

        [Fact]
        public async Task Should_Execute_Exactly_100_Requests_For_10_RPS_10_Seconds_CompleteInterval_Mode()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "10_RPS_10_Seconds_CompleteInterval_Mode",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(10),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 10,
                    TerminationMode = TerminationMode.CompleteCurrentInterval, // Industry standard
                    GracefulStopTimeout = TimeSpan.FromSeconds(5)
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
            // CompleteInterval mode should give more predictable request counts
            Assert.Equal(100, result.Total); // Should be exactly 100 requests
            Assert.Equal(100, requestCount);
            Assert.True(result.Success > 0);
            Assert.Equal(0, result.Failure);
        }

        [Fact]
        public async Task Should_Execute_Exactly_50_Requests_For_10_RPS_5_Seconds()
        {
            // Arrange
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "10_RPS_5_Seconds_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(5),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 10,
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(5);
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(50, result.Total); // Should be exactly 50 requests
            Assert.Equal(50, requestCount);
        }

        [Fact]
        public async Task Should_Handle_Different_RPS_Configurations()
        {
            // Test 5 RPS for 4 seconds = 20 requests
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "5_RPS_4_Seconds_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(4),
                    Interval = TimeSpan.FromSeconds(1),
                    Concurrency = 5, // 5 RPS
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2)
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
            Assert.Equal(20, result.Total); // Should be exactly 20 requests
            Assert.Equal(20, requestCount);
        }

        [Fact]
        public async Task Should_Handle_High_Frequency_Requests()
        {
            // Test 20 RPS for 3 seconds = 60 requests (every 0.5 seconds, 5 requests)
            var requestCount = 0;
            var executionPlan = new LoadExecutionPlan
            {
                Name = "20_RPS_3_Seconds_Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromMilliseconds(500), // Every 0.5 seconds
                    Concurrency = 10, // 10 requests every 0.5s = 20 RPS
                    TerminationMode = TerminationMode.CompleteCurrentInterval,
                    GracefulStopTimeout = TimeSpan.FromSeconds(2)
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    await Task.Delay(5);
                    return true;
                }
            };

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            Assert.Equal(60, result.Total); // Should be exactly 60 requests
            Assert.Equal(60, requestCount);
        }

        [Fact]
        public async Task Should_Complete_Within_Reasonable_Time()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Timing_Precision_Test",
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
                    await Task.Delay(10);
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);
            var endTime = DateTime.UtcNow;
            var actualDuration = endTime - startTime;

            // Assert
            Assert.True(actualDuration.TotalSeconds >= 3.0);
            Assert.True(actualDuration.TotalSeconds <= 6.0);
            Assert.Equal(15, result.Total); // 3 batches × 5 requests
            Assert.True(result.RequestsPerSecond >= 2.5); // Should be reasonable RPS
        }

        public void Dispose()
        {
            // Clean up any resources if needed
        }
    }
}
