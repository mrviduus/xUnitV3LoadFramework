using System.Collections.Concurrent;
using System.Diagnostics;
using Akka.Actor;
using Xunit;
using xUnitV3LoadFramework.LoadRunnerCore.Actors;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadTests
{
    public class LoadWorkerActorHybridTests : IDisposable
    {
        private readonly ActorSystem _system = ActorSystem.Create("HybridTestSystem");

        [Fact]
        public async Task HybridActor_ShouldHandleModerateLoad_WithCorrectTiming()
        {
            // Arrange - More reasonable test parameters
            var requestCount = 0;

            var executionPlan = new LoadExecutionPlan
            {
                Name = "ModerateLoadTest",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(3), // 3 seconds
                    Concurrency = 100, // 100 requests per batch
                    Interval = TimeSpan.FromMilliseconds(500) // Every 500ms = 600 total requests
                },
                Action = async () =>
                {
                    Interlocked.Increment(ref requestCount);
                    
                    // Quick task simulation
                    await Task.Delay(Random.Shared.Next(10, 50));
                    return true;
                }
            };

            var resultCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("test")));
            var loadWorkerActor = _system.ActorOf(Props.Create(() => 
                new LoadWorkerActorHybrid(executionPlan, resultCollector)));

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await loadWorkerActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(10));
            stopwatch.Stop();

            // Assert
            Assert.True(result.RequestsStarted >= 500); // Should be ~600 requests
            Assert.True(result.Total >= 450); // Allow for some variance due to async nature
            Assert.True(result.RequestsPerSecond > 100); // Should have decent throughput
            Assert.True(result.WorkerThreadsUsed > 0);
            Assert.True(result.BatchesCompleted >= 5); // ~6 batches expected
            Assert.True(result.AverageLatency > 0);
            Assert.True(result.MedianLatency > 0);
            Assert.True(result.Percentile95Latency > 0);
            Assert.True(result.Percentile99Latency > 0);
            
            Console.WriteLine($"Test completed in {stopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Requests Started: {result.RequestsStarted}");
            Console.WriteLine($"Requests Completed: {result.Total}");
            Console.WriteLine($"RPS: {result.RequestsPerSecond:F2}");
            Console.WriteLine($"Worker threads: {result.WorkerThreadsUsed}");
            Console.WriteLine($"Batches completed: {result.BatchesCompleted}");
        }

        [Fact]
        public async Task HybridActor_ShouldCalculatePercentilesCorrectly()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "PercentileTest",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Concurrency = 50,
                    Interval = TimeSpan.FromMilliseconds(200) // 5 batches = 250 requests
                },
                Action = async () =>
                {
                    // Create predictable latency distribution
                    var delay = Random.Shared.Next(10, 100);
                    await Task.Delay(delay);
                    return true;
                }
            };

            var resultCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("percentile")));
            var loadWorkerActor = _system.ActorOf(Props.Create(() => 
                new LoadWorkerActorHybrid(executionPlan, resultCollector)));

            // Act
            var result = await loadWorkerActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(8));

            // Assert
            Assert.True(result.MedianLatency > 0);
            Assert.True(result.MedianLatency <= result.Percentile95Latency);
            Assert.True(result.Percentile95Latency <= result.Percentile99Latency);
            Assert.True(result.MinLatency <= result.MedianLatency);
            Assert.True(result.MedianLatency <= result.MaxLatency);
            Assert.True(result.Total > 200); // Should complete most requests
            
            Console.WriteLine($"Latency distribution:");
            Console.WriteLine($"  Min: {result.MinLatency:F2}ms");
            Console.WriteLine($"  Median: {result.MedianLatency:F2}ms");
            Console.WriteLine($"  Avg: {result.AverageLatency:F2}ms");
            Console.WriteLine($"  P95: {result.Percentile95Latency:F2}ms");
            Console.WriteLine($"  P99: {result.Percentile99Latency:F2}ms");
            Console.WriteLine($"  Max: {result.MaxLatency:F2}ms");
        }

        [Fact]
        public async Task HybridActor_ShouldTrackResourceUtilization()
        {
            // Arrange
            var executionPlan = new LoadExecutionPlan
            {
                Name = "ResourceTest",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Concurrency = 100,
                    Interval = TimeSpan.FromMilliseconds(250) // 8 batches = 800 requests
                },
                Action = async () =>
                {
                    // Allocate some memory
                    var data = new byte[1024]; // 1KB
                    await Task.Delay(20);
                    return data.Length > 0;
                }
            };

            var resultCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("resource")));
            var loadWorkerActor = _system.ActorOf(Props.Create(() => 
                new LoadWorkerActorHybrid(executionPlan, resultCollector)));

            // Act
            var result = await loadWorkerActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(8));

            // Assert
            Assert.True(result.WorkerThreadsUsed > 0);
            Assert.True(result.PeakMemoryUsage > 0);
            Assert.True(result.WorkerUtilization >= 0);
            Assert.True(result.BatchesCompleted >= 7); // Should complete most batches
            Assert.True(result.Total > 600); // Should complete most requests
            
            Console.WriteLine($"Resource utilization:");
            Console.WriteLine($"  Worker threads: {result.WorkerThreadsUsed}");
            Console.WriteLine($"  Peak memory: {result.PeakMemoryUsage / 1024 / 1024:F2}MB");
            Console.WriteLine($"  Worker utilization: {result.WorkerUtilization:F2}");
            Console.WriteLine($"  Batches completed: {result.BatchesCompleted}");
        }

        [Fact]
        public async Task HybridActor_ShouldComparePerformance_WithTaskBasedActor()
        {
            // Test hybrid vs task-based performance on moderate load
            var hybridPlan = new LoadExecutionPlan
            {
                Name = "HybridComparison",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Concurrency = 50,
                    Interval = TimeSpan.FromMilliseconds(200)
                },
                Action = async () =>
                {
                    await Task.Delay(Random.Shared.Next(10, 30));
                    return true;
                }
            };

            var taskPlan = new LoadExecutionPlan
            {
                Name = "TaskComparison",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Concurrency = 50,
                    Interval = TimeSpan.FromMilliseconds(200)
                },
                Action = async () =>
                {
                    await Task.Delay(Random.Shared.Next(10, 30));
                    return true;
                }
            };

            // Test Hybrid
            var hybridCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("hybrid")));
            var hybridActor = _system.ActorOf(Props.Create(() => new LoadWorkerActorHybrid(hybridPlan, hybridCollector)));
            var hybridResult = await hybridActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(8));

            // Test Task-based
            var taskCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("task")));
            var taskActor = _system.ActorOf(Props.Create(() => new LoadWorkerActor(taskPlan, taskCollector)));
            var taskResult = await taskActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(8));

            // Both should complete similar number of requests
            var hybridCompleted = hybridResult.Total;
            var taskCompleted = taskResult.Total;
            var difference = Math.Abs(hybridCompleted - taskCompleted);
            var tolerance = Math.Max(hybridCompleted, taskCompleted) * 0.1; // 10% tolerance

            Assert.True(difference <= tolerance, 
                $"Hybrid completed {hybridCompleted}, Task completed {taskCompleted}, difference {difference} exceeds tolerance {tolerance}");

            Console.WriteLine("Performance Comparison:");
            Console.WriteLine($"Hybrid - Requests: {hybridResult.Total}, RPS: {hybridResult.RequestsPerSecond:F2}, Workers: {hybridResult.WorkerThreadsUsed}");
            Console.WriteLine($"Task   - Requests: {taskResult.Total}, RPS: {taskResult.RequestsPerSecond:F2}");
        }

        public void Dispose()
        {
            _system.Terminate().Wait(TimeSpan.FromSeconds(5));
            _system.Dispose();
        }
    }

    /// <summary>
    /// Intensive load tests that can be run separately for performance validation
    /// </summary>
    [Trait("Category", "LongRunning")]
    public class LoadWorkerActorHybrid500kTest : IDisposable
    {
        private readonly ActorSystem _system = ActorSystem.Create("HybridTestSystem500k");

        [Fact(Skip = "Long running test - run manually for 500k validation")]
        public async Task HybridActor_ShouldHandle500kRequests_WhenRunManually()
        {
            // This test is skipped by default but can be run manually
            // to verify 500k request handling capability
            
            var executionPlan = new LoadExecutionPlan
            {
                Name = "500kRequestTest",
                Settings = new LoadSettings
                {
                    Duration = TimeSpan.FromSeconds(50),
                    Concurrency = 10000,
                    Interval = TimeSpan.FromSeconds(1)
                },
                Action = async () =>
                {
                    await Task.Delay(Random.Shared.Next(50, 200));
                    return true;
                }
            };

            var resultCollector = _system.ActorOf(Props.Create(() => new ResultCollectorActor("500k")));
            var loadWorkerActor = _system.ActorOf(Props.Create(() => 
                new LoadWorkerActorHybrid(executionPlan, resultCollector)));

            var result = await loadWorkerActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromMinutes(2));

            Assert.Equal(500_000, result.RequestsStarted);
            Console.WriteLine($"500k test completed. RPS: {result.RequestsPerSecond:F2}");
        }

        public void Dispose()
        {
            _system.Terminate().Wait(TimeSpan.FromSeconds(5));
            _system.Dispose();
        }
    }
}
