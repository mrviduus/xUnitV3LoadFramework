using System.Text.Json;
using System.Text.Json.Serialization;

namespace xUnitV3LoadFramework.Extensions.Reports;

/// <summary>
/// Manages and aggregates multiple test runs into organized result sets
/// </summary>
public static class TestRunAggregator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Creates a run summary for all tests executed in a single run
    /// </summary>
    /// <param name="runId">The run identifier</param>
    /// <param name="testResults">Collection of test results for this run</param>
    /// <returns>Path to the run summary file</returns>
    public static async Task<string> CreateRunSummaryAsync(string runId, IEnumerable<LoadTestResult> testResults)
    {
        var baseDir = GetTestResultsDirectory();
        var runDir = Path.Combine(baseDir, $"Run_{runId}");
        Directory.CreateDirectory(runDir);

        var summary = new TestRunSummary
        {
            RunId = runId,
            Timestamp = DateTime.UtcNow,
            TestResults = testResults.ToList(),
            Statistics = CalculateRunStatistics(testResults)
        };

        var summaryPath = Path.Combine(runDir, "run_summary.json");
        var json = JsonSerializer.Serialize(summary, JsonOptions);
        await File.WriteAllTextAsync(summaryPath, json);

        return summaryPath;
    }

    /// <summary>
    /// Gets the TestResults directory, creating it if it doesn't exist
    /// </summary>
    private static string GetTestResultsDirectory()
    {
        // Find the project root by looking for the .sln file
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = FindProjectRoot(currentDir);
        
        var testResultsDir = Path.Combine(projectRoot, "TestResults");
        Directory.CreateDirectory(testResultsDir);
        
        return testResultsDir;
    }

    /// <summary>
    /// Finds the project root directory by looking for the solution file
    /// </summary>
    private static string FindProjectRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Any())
                return dir.FullName;
            
            dir = dir.Parent;
        }
        
        // Fallback to current directory if no solution file found
        return startDir;
    }

    /// <summary>
    /// Calculates aggregate statistics for a test run
    /// </summary>
    private static TestRunStatistics CalculateRunStatistics(IEnumerable<LoadTestResult> testResults)
    {
        var results = testResults.ToList();
        
        if (!results.Any())
        {
            return new TestRunStatistics();
        }

        var totalRequests = results.Sum(r => r.Results.Total);
        var totalSuccess = results.Sum(r => r.Results.Success);
        var totalFailures = results.Sum(r => r.Results.Failure);
        var allLatencies = results.SelectMany(r => new[] { r.Results.AverageLatency }).Where(l => l > 0);
        var maxLatency = results.Max(r => r.Results.MaxLatency);
        var minLatency = results.Where(r => r.Results.MinLatency > 0).Min(r => r.Results.MinLatency);
        var totalThroughput = results.Sum(r => r.Results.RequestsPerSecond);

        return new TestRunStatistics
        {
            TotalTests = results.Count,
            PassedTests = results.Count(r => r.Summary.Status == "PASSED"),
            FailedTests = results.Count(r => r.Summary.Status == "FAILED"),
            TotalRequests = totalRequests,
            TotalSuccessfulRequests = totalSuccess,
            TotalFailedRequests = totalFailures,
            OverallSuccessRate = totalRequests > 0 ? (totalSuccess / (double)totalRequests) * 100 : 0,
            OverallFailureRate = totalRequests > 0 ? (totalFailures / (double)totalRequests) * 100 : 0,
            AverageLatencyAcrossTests = allLatencies.Any() ? allLatencies.Average() : 0,
            MaxLatencyAcrossTests = maxLatency,
            MinLatencyAcrossTests = minLatency,
            TotalThroughput = totalThroughput,
            TestDuration = results.Max(r => r.Results.Time)
        };
    }
}

/// <summary>
/// Summary of an entire test run containing multiple tests
/// </summary>
public class TestRunSummary
{
    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("testResults")]
    public List<LoadTestResult> TestResults { get; set; } = new();

    [JsonPropertyName("statistics")]
    public TestRunStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Aggregate statistics for a test run
/// </summary>
public class TestRunStatistics
{
    [JsonPropertyName("totalTests")]
    public int TotalTests { get; set; }

    [JsonPropertyName("passedTests")]
    public int PassedTests { get; set; }

    [JsonPropertyName("failedTests")]
    public int FailedTests { get; set; }

    [JsonPropertyName("totalRequests")]
    public int TotalRequests { get; set; }

    [JsonPropertyName("totalSuccessfulRequests")]
    public int TotalSuccessfulRequests { get; set; }

    [JsonPropertyName("totalFailedRequests")]
    public int TotalFailedRequests { get; set; }

    [JsonPropertyName("overallSuccessRate")]
    public double OverallSuccessRate { get; set; }

    [JsonPropertyName("overallFailureRate")]
    public double OverallFailureRate { get; set; }

    [JsonPropertyName("averageLatencyAcrossTests")]
    public double AverageLatencyAcrossTests { get; set; }

    [JsonPropertyName("maxLatencyAcrossTests")]
    public double MaxLatencyAcrossTests { get; set; }

    [JsonPropertyName("minLatencyAcrossTests")]
    public double MinLatencyAcrossTests { get; set; }

    [JsonPropertyName("totalThroughput")]
    public double TotalThroughput { get; set; }

    [JsonPropertyName("testDuration")]
    public double TestDuration { get; set; }
}
