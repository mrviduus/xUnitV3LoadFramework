using System;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;
using xUnitV3LoadFramework.LoadRunnerCore.Configuration;
using Akka.Actor;

namespace xUnitV3LoadFrameworkTests.Unit
{
    /// <summary>
    /// Tests to reproduce and verify the Akka.ActorTimeoutException issue
    /// These tests demonstrate the problem and validate the fix
    /// </summary>
    public class LoadRunnerTimeoutTests : IDisposable
    {
        private readonly ActorSystem _actorSystem;

        public LoadRunnerTimeoutTests()
        {
            _actorSystem = ActorSystem.Create("LoadTestSystem");
        }

        [Fact]
        public async Task LoadRunner_Should_Complete_Simple_Test_Without_Timeout()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Simple Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromMilliseconds(500),
                    Concurrency = 1
                },
                Action = async () =>
                {
                    await Task.Delay(10);
                    return true;
                }
            };

            // Act & Assert - should not throw timeout
            var result = await LoadRunner.Run(executionPlan);
            
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
            Assert.True(result.Success > 0);
        }

        [Fact]
        public async Task LoadRunner_Should_Handle_Multiple_Concurrent_Tasks()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Multiple Concurrent Tasks Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromMilliseconds(200),
                    Concurrency = 5
                },
                Action = async () =>
                {
                    await Task.Delay(50);
                    return true;
                }
            };

            // Act & Assert
            var result = await LoadRunner.Run(executionPlan);
            
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
            Assert.True(result.Success > 0);
        }

        [Fact]
        public async Task LoadRunner_Should_Handle_Slow_Actions_Gracefully()
        {
            // Arrange - this test might expose the timeout issue
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Slow Actions Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Interval = TimeSpan.FromMilliseconds(500),
                    Concurrency = 2
                },
                Action = async () =>
                {
                    await Task.Delay(200); // Slower action
                    return true;
                }
            };

            // Act & Assert
            var result = await LoadRunner.Run(executionPlan);
            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LoadRunner_Should_Handle_Fast_High_Concurrency()
        {
            // Arrange - this test stresses the actor system
            var executionPlan = new LoadExecutionPlan
            {
                Name = "High Concurrency Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromMilliseconds(100),
                    Concurrency = 10
                },
                Action = async () =>
                {
                    await Task.Delay(1);
                    return true;
                }
            };

            // Act & Assert
            var result = await LoadRunner.Run(executionPlan);
            
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task LoadRunner_Should_Handle_Action_That_Throws_Exception()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Exception Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Interval = TimeSpan.FromMilliseconds(500),
                    Concurrency = 1
                },
                Action = async () =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Test exception");
                }
            };

            // Act & Assert
            var result = await LoadRunner.Run(executionPlan);
            
            Assert.NotNull(result);
            Assert.True(result.Failure > 0);
        }

        [Fact]
        public async Task LoadRunner_Should_Complete_Within_Reasonable_Time()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Timing Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Interval = TimeSpan.FromMilliseconds(200),
                    Concurrency = 2
                },
                Action = async () =>
                {
                    await Task.Delay(5);
                    return true;
                }
            };

            var startTime = DateTime.UtcNow;

            // Act
            var result = await LoadRunner.Run(executionPlan);

            // Assert
            var totalTime = DateTime.UtcNow - startTime;
            
            Assert.NotNull(result);
            // Should complete within reasonable time (test duration + overhead)
            Assert.True(totalTime.TotalSeconds < 10, $"Test took too long: {totalTime.TotalSeconds} seconds");
        }

        [Fact]
        public async Task LoadRunner_With_Hybrid_Mode_Should_Not_Timeout()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "Hybrid Mode Test",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Interval = TimeSpan.FromMilliseconds(100),
                    Concurrency = 5
                },
                Action = async () =>
                {
                    await Task.Delay(10);
                    return true;
                }
            };

            var configuration = new LoadWorkerConfiguration
            {
                Mode = LoadWorkerMode.Hybrid
            };

            // Act & Assert
            var result = await LoadRunner.Run(executionPlan, configuration);
            
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        public void Dispose()
        {
            _actorSystem?.Terminate().Wait(TimeSpan.FromSeconds(5));
        }
    }
}
