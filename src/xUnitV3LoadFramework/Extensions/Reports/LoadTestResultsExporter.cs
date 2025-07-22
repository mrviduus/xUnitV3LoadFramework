using System.Text.Json;
using System.Text.Json.Serialization;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.Extensions.Reports;

/// <summary>
/// Exports load test results to JSON files in the TestResults folder
/// </summary>
public static class LoadTestResultsExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Exports load test results to a JSON file in the TestResults folder
    /// </summary>
    /// <param name="testName">Name of the test</param>
    /// <param name="loadResult">Load test results</param>
    /// <param name="testConfiguration">Optional test configuration details</param>
    /// <returns>Path to the generated JSON file</returns>
    public static async Task<string> ExportResultsAsync(
        string testName, 
        LoadResult loadResult, 
        TestConfigurationInfo? testConfiguration = null)
    {
        var timestamp = DateTime.UtcNow;
        var runId = timestamp.ToString("yyyyMMdd_HHmmss");
        
        // Create TestResults directory structure
        var baseDir = GetTestResultsDirectory();
        var runDir = Path.Combine(baseDir, $"Run_{runId}");
        Directory.CreateDirectory(runDir);

        // Generate safe filename
        var safeTestName = GetSafeFileName(testName);
        var fileName = $"{safeTestName}_{timestamp:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(runDir, fileName);

        // Create comprehensive test result
        var testResult = new LoadTestResult
        {
            TestName = testName,
            Timestamp = timestamp,
            RunId = runId,
            Results = loadResult,
            Configuration = testConfiguration ?? new TestConfigurationInfo(),
            Environment = new EnvironmentInfo(),
            Summary = CreateSummary(loadResult)
        };

        // Serialize and save
        var json = JsonSerializer.Serialize(testResult, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);

        return filePath;
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
    /// Creates a safe filename from test name
    /// </summary>
    private static string GetSafeFileName(string testName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safeName = string.Join("_", testName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        
        // Limit length and ensure it's not empty
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "LoadTest";
        
        if (safeName.Length > 100)
            safeName = safeName.Substring(0, 100);
            
        return safeName;
    }

    /// <summary>
    /// Creates a summary of the load test results
    /// </summary>
    private static LoadTestSummary CreateSummary(LoadResult result)
    {
        var successRate = result.Total > 0 ? (result.Success / (double)result.Total) * 100 : 0;
        var failureRate = result.Total > 0 ? (result.Failure / (double)result.Total) * 100 : 0;

        return new LoadTestSummary
        {
            TotalRequests = result.Total,
            SuccessfulRequests = result.Success,
            FailedRequests = result.Failure,
            SuccessRate = successRate,
            FailureRate = failureRate,
            AverageLatency = result.AverageLatency,
            MaxLatency = result.MaxLatency,
            MinLatency = result.MinLatency,
            Percentile95 = result.Percentile95Latency,
            Percentile99 = result.Percentile99Latency,
            ThroughputRps = result.RequestsPerSecond,
            TestDuration = result.Time,
            Status = result.Failure > 0 ? "FAILED" : "PASSED"
        };
    }
}

/// <summary>
/// Comprehensive load test result container
/// </summary>
public class LoadTestResult
{
    [JsonPropertyName("testName")]
    public string TestName { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public LoadResult Results { get; set; } = null!;

    [JsonPropertyName("configuration")]
    public TestConfigurationInfo Configuration { get; set; } = new();

    [JsonPropertyName("environment")]
    public EnvironmentInfo Environment { get; set; } = new();

    [JsonPropertyName("summary")]
    public LoadTestSummary Summary { get; set; } = new();
}

/// <summary>
/// Test configuration information
/// </summary>
public class TestConfigurationInfo
{
    [JsonPropertyName("concurrency")]
    public int Concurrency { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }

    [JsonPropertyName("testMethod")]
    public string TestMethod { get; set; } = string.Empty;

    [JsonPropertyName("testClass")]
    public string TestClass { get; set; } = string.Empty;

    [JsonPropertyName("assembly")]
    public string Assembly { get; set; } = string.Empty;
}

/// <summary>
/// Environment and system information
/// </summary>
public class EnvironmentInfo
{
    [JsonPropertyName("machineName")]
    public string MachineName { get; set; } = System.Environment.MachineName;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = System.Environment.UserName;

    [JsonPropertyName("osVersion")]
    public string OsVersion { get; set; } = System.Environment.OSVersion.ToString();

    [JsonPropertyName("processorCount")]
    public int ProcessorCount { get; set; } = System.Environment.ProcessorCount;

    [JsonPropertyName("frameworkVersion")]
    public string FrameworkVersion { get; set; } = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

    [JsonPropertyName("workingSet")]
    public long WorkingSet { get; set; } = System.Environment.WorkingSet;
}

/// <summary>
/// Summary of load test results
/// </summary>
public class LoadTestSummary
{
    [JsonPropertyName("totalRequests")]
    public int TotalRequests { get; set; }

    [JsonPropertyName("successfulRequests")]
    public int SuccessfulRequests { get; set; }

    [JsonPropertyName("failedRequests")]
    public int FailedRequests { get; set; }

    [JsonPropertyName("successRate")]
    public double SuccessRate { get; set; }

    [JsonPropertyName("failureRate")]
    public double FailureRate { get; set; }

    [JsonPropertyName("averageLatency")]
    public double AverageLatency { get; set; }

    [JsonPropertyName("maxLatency")]
    public double MaxLatency { get; set; }

    [JsonPropertyName("minLatency")]
    public double MinLatency { get; set; }

    [JsonPropertyName("percentile95")]
    public double Percentile95 { get; set; }

    [JsonPropertyName("percentile99")]
    public double Percentile99 { get; set; }

    [JsonPropertyName("throughputRps")]
    public double ThroughputRps { get; set; }

    [JsonPropertyName("testDuration")]
    public double TestDuration { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "UNKNOWN";
}
