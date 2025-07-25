using xUnitV3LoadFramework.Extensions.Runners;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.Extensions.Reports;

/// <summary>
/// Handles the generation of comprehensive load test result reports for console output
/// </summary>
internal class LoadTestResultReport
{
	private const string DATE_FORMAT = "yyyyMMdd_HHmmss";
	private const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";
	
	private readonly LoadTestRunnerContext _context;
	private readonly LoadResult _result;
	private readonly DateTime _timestamp;
	private readonly string _runId;
	private readonly LoadTestStatistics _statistics;

	public LoadTestResultReport(LoadTestRunnerContext context, LoadResult result)
	{
		_context = context;
		_result = result;
		_timestamp = DateTime.UtcNow;
		_runId = _timestamp.ToString(DATE_FORMAT);
		_statistics = new LoadTestStatistics(result);
	}

	public string GenerateSummaryMessage()
	{
		return string.Join("\n",
			GenerateHeader(),
			GenerateResultsSection(),
			GenerateConfigurationSection(),
			GenerateEnvironmentSection(),
			GenerateSummarySection()
		);
	}

	private string GenerateHeader()
	{
		return $"[LOAD TEST RESULT] {_context.Test.TestDisplayName}:\n" +
			   $"- Timestamp: {_timestamp.ToString(TIMESTAMP_FORMAT)} UTC\n" +
			   $"- Run ID: {_runId}";
	}

	private string GenerateResultsSection()
	{
		return "\n=== RESULTS ===" +
			   $"\n- Scenario Name: {_result.ScenarioName}" +
			   $"\n- Total Executions: {_result.Total}" +
			   $"\n- Success: {_result.Success}" +
			   $"\n- Failure: {_result.Failure}" +
			   $"\n- Requests Started: {_result.RequestsStarted}" +
			   $"\n- Requests In-Flight: {_result.RequestsInFlight}" +
			   $"\n- Time: {_result.Time:F2} s" +
			   GenerateLatencyMetrics() +
			   GeneratePerformanceMetrics() +
			   GenerateResourceMetrics();
	}

	private string GenerateLatencyMetrics()
	{
		return $"\n- Max Latency: {_result.MaxLatency:F2} ms" +
			   $"\n- Min Latency: {_result.MinLatency:F2} ms" +
			   $"\n- Average Latency: {_result.AverageLatency:F2} ms" +
			   $"\n- Median Latency: {_result.MedianLatency:F2} ms" +
			   $"\n- 95th Percentile Latency: {_result.Percentile95Latency:F2} ms" +
			   $"\n- 99th Percentile Latency: {_result.Percentile99Latency:F2} ms";
	}

	private string GeneratePerformanceMetrics()
	{
		return $"\n- Requests Per Second: {_result.RequestsPerSecond:F2}" +
			   $"\n- Avg Queue Time: {_result.AvgQueueTime:F2} ms" +
			   $"\n- Max Queue Time: {_result.MaxQueueTime:F2} ms";
	}

	private string GenerateResourceMetrics()
	{
		return $"\n- Worker Threads Used: {_result.WorkerThreadsUsed}" +
			   $"\n- Worker Utilization: {_result.WorkerUtilization:P2}" +
			   $"\n- Peak Memory Usage: {ByteFormatter.FormatBytes(_result.PeakMemoryUsage)}" +
			   $"\n- Batches Completed: {_result.BatchesCompleted}";
	}

	private string GenerateConfigurationSection()
	{
		var testCase = _context.Test.TestCase;
		var testMethod = testCase.TestMethod;
		var testClass = testMethod.TestClass;
		var assembly = testClass.TestCollection.TestAssembly.Assembly;

		return "\n=== CONFIGURATION ===" +
			   $"\n- Concurrency: {testCase.Concurrency}" +
			   $"\n- Duration: {testCase.Duration} ms" +
			   $"\n- Interval: {testCase.Interval} ms" +
			   $"\n- Test Method: {testMethod.Method.Name}" +
			   $"\n- Test Class: {testClass.Class.Name}" +
			   $"\n- Assembly: {assembly.GetName().Name ?? "Unknown"}";
	}

	private string GenerateEnvironmentSection()
	{
		return "\n=== ENVIRONMENT ===" +
			   $"\n- Machine Name: {Environment.MachineName}" +
			   $"\n- User Name: {Environment.UserName}" +
			   $"\n- OS Version: {Environment.OSVersion}" +
			   $"\n- Processor Count: {Environment.ProcessorCount}" +
			   $"\n- Framework Version: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}" +
			   $"\n- Working Set: {ByteFormatter.FormatBytes(Environment.WorkingSet)}";
	}

	private string GenerateSummarySection()
	{
		return "\n=== SUMMARY ===" +
			   $"\n- Status: {_statistics.Status}" +
			   $"\n- Success Rate: {_statistics.SuccessRate:F1}%" +
			   $"\n- Failure Rate: {_statistics.FailureRate:F1}%" +
			   $"\n- Throughput: {_result.RequestsPerSecond:F2} RPS" +
			   $"\n- Test Duration: {_result.Time:F2} seconds";
	}
}

/// <summary>
/// Calculates statistical information about load test results
/// </summary>
internal class LoadTestStatistics
{
	public double SuccessRate { get; }
	public double FailureRate { get; }
	public string Status { get; }

	public LoadTestStatistics(LoadResult result)
	{
		SuccessRate = result.Total > 0 ? (result.Success / (double)result.Total) * 100 : 0;
		FailureRate = result.Total > 0 ? (result.Failure / (double)result.Total) * 100 : 0;
		Status = result.Failure > 0 ? "FAILED" : "PASSED";
	}
}

/// <summary>
/// Utility class for formatting byte values in human-readable format
/// </summary>
internal static class ByteFormatter
{
	private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB" };

	public static string FormatBytes(long bytes)
	{
		int order = 0;
		double size = bytes;

		while (size >= 1024 && order < SizeUnits.Length - 1)
		{
			order++;
			size /= 1024;
		}

		return $"{size:F2} {SizeUnits[order]}";
	}
}
