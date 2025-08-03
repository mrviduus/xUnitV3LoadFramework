using xUnitV3LoadFramework.LoadRunnerCore.Configuration;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

partial class Program
{
    static async Task TestTaskBasedVsHybrid()
    {
        var executionPlan = new LoadExecutionPlan
        {
            Name = "ComparisonTest",
            Settings = new LoadSettings
            {
                Duration = TimeSpan.FromSeconds(10),
                Concurrency = 1000, // 1k requests per batch
                Interval = TimeSpan.FromMilliseconds(1000) // Every second
            },
            Action = async () =>
            {
                // Simulate work
                await Task.Delay(Random.Shared.Next(50, 150));
                return true;
            }
        };

        // Test Task-based implementation
        Console.WriteLine("\n=== Testing Task-Based Implementation ===");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var taskBasedConfig = new LoadWorkerConfiguration
        {
            Mode = LoadWorkerMode.TaskBased
        };
        
        var taskBasedResult = await LoadRunner.Run(executionPlan, taskBasedConfig);
        stopwatch.Stop();

        Console.WriteLine($"Task-Based Results:");
        Console.WriteLine($"  Duration: {stopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Requests Started: {taskBasedResult.RequestsStarted:N0}");
        Console.WriteLine($"  Requests Completed: {taskBasedResult.Total:N0}");
        Console.WriteLine($"  Success Rate: {(taskBasedResult.Success / (double)taskBasedResult.Total * 100):F2}%");
        Console.WriteLine($"  Avg Latency: {taskBasedResult.AverageLatency:F2}ms");
        Console.WriteLine($"  P95 Latency: {taskBasedResult.Percentile95Latency:F2}ms");
        Console.WriteLine($"  Requests/sec: {taskBasedResult.RequestsPerSecond:F0}");

        // Test Hybrid implementation
        Console.WriteLine("\n=== Testing Hybrid Implementation ===");
        stopwatch.Restart();
        
        var hybridConfig = new LoadWorkerConfiguration
        {
            Mode = LoadWorkerMode.Hybrid
        };
        
        var hybridResult = await LoadRunner.Run(executionPlan, hybridConfig);
        stopwatch.Stop();

        Console.WriteLine($"Hybrid Results:");
        Console.WriteLine($"  Duration: {stopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  Requests Started: {hybridResult.RequestsStarted:N0}");
        Console.WriteLine($"  Requests Completed: {hybridResult.Total:N0}");
        Console.WriteLine($"  Success Rate: {(hybridResult.Success / (double)hybridResult.Total * 100):F2}%");
        Console.WriteLine($"  Avg Latency: {hybridResult.AverageLatency:F2}ms");
        Console.WriteLine($"  P95 Latency: {hybridResult.Percentile95Latency:F2}ms");
        Console.WriteLine($"  P99 Latency: {hybridResult.Percentile99Latency:F2}ms");
        Console.WriteLine($"  Median Latency: {hybridResult.MedianLatency:F2}ms");
        Console.WriteLine($"  Requests/sec: {hybridResult.RequestsPerSecond:F0}");
        Console.WriteLine($"  Avg Queue Time: {hybridResult.AvgQueueTime:F2}ms");
        Console.WriteLine($"  Max Queue Time: {hybridResult.MaxQueueTime:F2}ms");
        Console.WriteLine($"  Peak Memory: {hybridResult.PeakMemoryUsage / 1024 / 1024:F2}MB");

        // Compare accuracy
        var expectedRequests = (int)(executionPlan.Settings.Duration.TotalSeconds / executionPlan.Settings.Interval.TotalSeconds * executionPlan.Settings.Concurrency);
        
        Console.WriteLine($"\n=== Comparison ===");
        Console.WriteLine($"Expected Requests: {expectedRequests:N0}");
        Console.WriteLine($"Task-Based Accuracy: {(taskBasedResult.RequestsStarted / (double)expectedRequests * 100):F2}%");
        Console.WriteLine($"Hybrid Accuracy: {(hybridResult.RequestsStarted / (double)expectedRequests * 100):F2}%");
        
        var winner = hybridResult.RequestsStarted >= taskBasedResult.RequestsStarted ? "Hybrid" : "Task-Based";
        Console.WriteLine($"Better Accuracy: {winner}");
    }
}
