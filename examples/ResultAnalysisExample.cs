using System.Text.Json;
using xUnitV3LoadFramework.Extensions.Reports;

namespace xUnitV3LoadFramework.Examples;

/// <summary>
/// Example demonstrating how to programmatically analyze JSON test results
/// </summary>
public static class ResultAnalysisExample
{
    /// <summary>
    /// Analyzes all test results in the TestResults folder and generates a summary report
    /// </summary>
    public static async Task AnalyzeTestResults()
    {
        var testResultsDir = Path.Combine(GetProjectRoot(), "TestResults");
        
        if (!Directory.Exists(testResultsDir))
        {
            Console.WriteLine("No TestResults folder found.");
            return;
        }

        var runDirs = Directory.GetDirectories(testResultsDir, "Run_*");
        Console.WriteLine($"Found {runDirs.Length} test runs");
        Console.WriteLine();

        foreach (var runDir in runDirs.OrderBy(d => d))
        {
            await AnalyzeTestRun(runDir);
        }
    }

    /// <summary>
    /// Analyzes a single test run and displays summary information
    /// </summary>
    private static async Task AnalyzeTestRun(string runDir)
    {
        var runName = Path.GetFileName(runDir);
        var jsonFiles = Directory.GetFiles(runDir, "*.json");
        
        Console.WriteLine($"=== {runName} ===");
        Console.WriteLine($"Tests in run: {jsonFiles.Length}");

        var allResults = new List<LoadTestResult>();
        
        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(jsonFile);
                var result = JsonSerializer.Deserialize<LoadTestResult>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                if (result != null)
                {
                    allResults.Add(result);
                    DisplayTestSummary(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {jsonFile}: {ex.Message}");
            }
        }

        if (allResults.Any())
        {
            DisplayRunSummary(allResults);
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Displays summary for a single test
    /// </summary>
    private static void DisplayTestSummary(LoadTestResult result)
    {
        var status = result.Summary.Status == "PASSED" ? "✅" : "❌";
        Console.WriteLine($"  {status} {result.TestName}");
        Console.WriteLine($"     Success Rate: {result.Summary.SuccessRate:F1}% | " +
                         $"Avg Latency: {result.Summary.AverageLatency:F2}ms | " +
                         $"Throughput: {result.Summary.ThroughputRps:F1} RPS | " +
                         $"Duration: {result.Summary.TestDuration:F1}s");
    }

    /// <summary>
    /// Displays aggregate summary for all tests in a run
    /// </summary>
    private static void DisplayRunSummary(List<LoadTestResult> results)
    {
        var totalTests = results.Count;
        var passedTests = results.Count(r => r.Summary.Status == "PASSED");
        var failedTests = results.Count(r => r.Summary.Status == "FAILED");
        var totalRequests = results.Sum(r => r.Summary.TotalRequests);
        var totalSuccessful = results.Sum(r => r.Summary.SuccessfulRequests);
        var overallSuccessRate = totalRequests > 0 ? (totalSuccessful / (double)totalRequests) * 100 : 0;
        var avgLatency = results.Where(r => r.Summary.AverageLatency > 0).Average(r => r.Summary.AverageLatency);
        var totalThroughput = results.Sum(r => r.Summary.ThroughputRps);

        Console.WriteLine("  --- Run Summary ---");
        Console.WriteLine($"  Tests: {totalTests} ({passedTests} passed, {failedTests} failed)");
        Console.WriteLine($"  Total Requests: {totalRequests:N0}");
        Console.WriteLine($"  Overall Success Rate: {overallSuccessRate:F1}%");
        Console.WriteLine($"  Average Latency: {avgLatency:F2}ms");
        Console.WriteLine($"  Total Throughput: {totalThroughput:F1} RPS");
    }

    /// <summary>
    /// Finds the project root directory
    /// </summary>
    private static string GetProjectRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Any())
                return dir.FullName;
            
            dir = dir.Parent;
        }
        
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Example: Find tests that exceed latency thresholds
    /// </summary>
    public static async Task<List<string>> FindSlowTests(double latencyThresholdMs = 100.0)
    {
        var testResultsDir = Path.Combine(GetProjectRoot(), "TestResults");
        var slowTests = new List<string>();
        
        if (!Directory.Exists(testResultsDir))
            return slowTests;

        var jsonFiles = Directory.GetFiles(testResultsDir, "*.json", SearchOption.AllDirectories);
        
        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(jsonFile);
                var result = JsonSerializer.Deserialize<LoadTestResult>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                if (result?.Summary.AverageLatency > latencyThresholdMs)
                {
                    slowTests.Add($"{result.TestName}: {result.Summary.AverageLatency:F2}ms avg latency");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing {jsonFile}: {ex.Message}");
            }
        }

        return slowTests;
    }

    /// <summary>
    /// Example: Generate performance trend analysis
    /// </summary>
    public static async Task GeneratePerformanceTrend(string testName)
    {
        var testResultsDir = Path.Combine(GetProjectRoot(), "TestResults");
        if (!Directory.Exists(testResultsDir))
            return;

        var matchingResults = new List<(DateTime timestamp, LoadTestResult result)>();
        var jsonFiles = Directory.GetFiles(testResultsDir, "*.json", SearchOption.AllDirectories);
        
        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(jsonFile);
                var result = JsonSerializer.Deserialize<LoadTestResult>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                if (result?.TestName.Contains(testName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    matchingResults.Add((result.Timestamp, result));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {jsonFile}: {ex.Message}");
            }
        }

        if (!matchingResults.Any())
        {
            Console.WriteLine($"No results found for test containing: {testName}");
            return;
        }

        Console.WriteLine($"=== Performance Trend for '{testName}' ===");
        Console.WriteLine("Timestamp                 | Avg Latency | Success Rate | Throughput");
        Console.WriteLine("--------------------------|-------------|--------------|------------");
        
        foreach (var (timestamp, result) in matchingResults.OrderBy(r => r.timestamp))
        {
            Console.WriteLine($"{timestamp:yyyy-MM-dd HH:mm:ss} | " +
                             $"{result.Summary.AverageLatency,10:F2}ms | " +
                             $"{result.Summary.SuccessRate,11:F1}% | " +
                             $"{result.Summary.ThroughputRps,9:F1} RPS");
        }
    }
}

/// <summary>
/// Console application example for running result analysis
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Load Test Results Analysis");
        Console.WriteLine("==========================");
        
        // Analyze all test results
        await ResultAnalysisExample.AnalyzeTestResults();
        
        // Find slow tests
        Console.WriteLine("Slow Tests (>50ms avg latency):");
        var slowTests = await ResultAnalysisExample.FindSlowTests(50.0);
        foreach (var slowTest in slowTests)
        {
            Console.WriteLine($"  ⚠️  {slowTest}");
        }
        
        if (!slowTests.Any())
        {
            Console.WriteLine("  ✅ No slow tests found!");
        }
        
        Console.WriteLine();
        
        // Generate trend for a specific test
        Console.WriteLine("Generating trend analysis...");
        await ResultAnalysisExample.GeneratePerformanceTrend("scenario");
    }
}
