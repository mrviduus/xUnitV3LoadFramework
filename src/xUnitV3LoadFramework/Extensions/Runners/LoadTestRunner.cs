using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;
using xUnitV3LoadFramework.Extensions.Reports;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestRunner :
	TestRunner<LoadTestRunnerContext, LoadTest>
{
	public static LoadTestRunner Instance { get; } = new();

	protected override ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(LoadTestRunnerContext ctxt) =>
		throw new NotSupportedException();

	protected override bool IsTestClassCreatable(LoadTestRunnerContext ctxt) =>
		false;

	protected override bool IsTestClassDisposable(
		LoadTestRunnerContext ctxt,
		object testClassInstance) =>
		false;

	protected override ValueTask<TimeSpan> InvokeTest(
		LoadTestRunnerContext ctxt,
		object? testClassInstance) =>
		base.InvokeTest(ctxt, ctxt.Specification);

	public async ValueTask<RunSummary> Run(
		Specification specification,
		LoadTest test,
		IMessageBus messageBus,
		string? skipReason,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{

		await using var ctxt = new LoadTestRunnerContext(specification, test, messageBus, skipReason, aggregator, cancellationTokenSource);
		if (!string.IsNullOrEmpty(skipReason))
		{
			await OnTestSkipped(ctxt, skipReason, 0m, "", null );
			return new RunSummary { Total = 1, Skipped = 1 };
		}
		await ctxt.InitializeAsync();
		
		var loadSettings = CreateLoadSettings(test);

		await OnTestStarting(ctxt);

		var summary = new RunSummary { Total = 1 };

		var executionPlan = CreateExecutionPlan(ctxt, loadSettings);
		var loadResult = await LoadRunner.Run(executionPlan);

		var reportLoadResult = await ReportLoadResult(ctxt, loadResult);

		if (loadResult.Failure > 0)
		{
			summary.Failed = loadResult.Failure;
			var exception = new Exception($"{loadResult.Failure} load test(s) failed.");
			await OnTestFailed(ctxt, exception, summary.Time, reportLoadResult, null);
		}
		else
		{
			await OnTestPassed(ctxt, summary.Time, reportLoadResult, null);
		}

		await OnTestFinished(ctxt, summary.Time, reportLoadResult, null, null);

		return summary;
	}

	private async Task<string> ReportLoadResult(LoadTestRunnerContext ctxt, LoadResult result)
	{
		// Calculate summary statistics (same as in JSON)
		var timestamp = DateTime.UtcNow;
		var runId = timestamp.ToString("yyyyMMdd_HHmmss");
		var successRate = result.Total > 0 ? (result.Success / (double)result.Total) * 100 : 0;
		var failureRate = result.Total > 0 ? (result.Failure / (double)result.Total) * 100 : 0;
		var status = result.Failure > 0 ? "FAILED" : "PASSED";
		
		// Build comprehensive summary message matching JSON structure
		var summaryMessage =
			$"[LOAD TEST RESULT] {ctxt.Test.TestDisplayName}:\n" +
			$"- Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss} UTC\n" +
			$"- Run ID: {runId}\n" +
			$"\n=== RESULTS ===\n" +
			$"- Scenario Name: {result.ScenarioName}\n" +
			$"- Total Executions: {result.Total}\n" +
			$"- Success: {result.Success}\n" +
			$"- Failure: {result.Failure}\n" +
			$"- Requests Started: {result.RequestsStarted}\n" +
			$"- Requests In-Flight: {result.RequestsInFlight}\n" +
			$"- Time: {result.Time:F2} s\n" +
			$"- Max Latency: {result.MaxLatency:F2} ms\n" +
			$"- Min Latency: {result.MinLatency:F2} ms\n" +
			$"- Average Latency: {result.AverageLatency:F2} ms\n" +
			$"- Median Latency: {result.MedianLatency:F2} ms\n" +
			$"- 95th Percentile Latency: {result.Percentile95Latency:F2} ms\n" +
			$"- 99th Percentile Latency: {result.Percentile99Latency:F2} ms\n" +
			$"- Requests Per Second: {result.RequestsPerSecond:F2}\n" +
			$"- Avg Queue Time: {result.AvgQueueTime:F2} ms\n" +
			$"- Max Queue Time: {result.MaxQueueTime:F2} ms\n" +
			$"- Worker Threads Used: {result.WorkerThreadsUsed}\n" +
			$"- Worker Utilization: {result.WorkerUtilization:P2}\n" +
			$"- Peak Memory Usage: {FormatBytes(result.PeakMemoryUsage)}\n" +
			$"- Batches Completed: {result.BatchesCompleted}\n" +
			$"\n=== CONFIGURATION ===\n" +
			$"- Concurrency: {ctxt.Test.TestCase.Concurrency}\n" +
			$"- Duration: {ctxt.Test.TestCase.Duration} ms\n" +
			$"- Interval: {ctxt.Test.TestCase.Interval} ms\n" +
			$"- Test Method: {ctxt.Test.TestCase.TestMethod.Method.Name}\n" +
			$"- Test Class: {ctxt.Test.TestCase.TestMethod.TestClass.Class.Name}\n" +
			$"- Assembly: {ctxt.Test.TestCase.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.GetName().Name ?? "Unknown"}\n" +
			$"\n=== ENVIRONMENT ===\n" +
			$"- Machine Name: {Environment.MachineName}\n" +
			$"- User Name: {Environment.UserName}\n" +
			$"- OS Version: {Environment.OSVersion}\n" +
			$"- Processor Count: {Environment.ProcessorCount}\n" +
			$"- Framework Version: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
			$"- Working Set: {FormatBytes(Environment.WorkingSet)}\n" +
			$"\n=== SUMMARY ===\n" +
			$"- Status: {status}\n" +
			$"- Success Rate: {successRate:F1}%\n" +
			$"- Failure Rate: {failureRate:F1}%\n" +
			$"- Throughput: {result.RequestsPerSecond:F2} RPS\n" +
			$"- Test Duration: {result.Time:F2} seconds";

		ctxt.MessageBus.QueueMessage(new DiagnosticMessage(summaryMessage));

		// Export results to JSON file
		try
		{
			var testConfiguration = new TestConfigurationInfo
			{
				Concurrency = ctxt.Test.TestCase.Concurrency,
				Duration = ctxt.Test.TestCase.Duration,
				Interval = ctxt.Test.TestCase.Interval,
				TestMethod = ctxt.Test.TestCase.TestMethod.Method.Name,
				TestClass = ctxt.Test.TestCase.TestMethod.TestClass.Class.Name,
				Assembly = ctxt.Test.TestCase.TestMethod.TestClass.TestCollection.TestAssembly.Assembly.GetName().Name ?? "Unknown"
			};

			var jsonFilePath = await LoadTestResultsExporter.ExportResultsAsync(
				ctxt.Test.TestDisplayName,
				result,
				testConfiguration
			);

			ctxt.MessageBus.QueueMessage(new DiagnosticMessage($"\nResults saved to: {jsonFilePath}"));
		}
		catch (Exception ex)
		{
			ctxt.MessageBus.QueueMessage(new DiagnosticMessage($"\nFailed to export JSON results: {ex.Message}"));
		}

		return summaryMessage;
	}

	private static string FormatBytes(long bytes)
	{
		string[] sizes = { "B", "KB", "MB", "GB", "TB" };
		int order = 0;
		double size = bytes;
		
		while (size >= 1024 && order < sizes.Length - 1)
		{
			order++;
			size = size / 1024;
		}
		
		return $"{size:F2} {sizes[order]}";
	}

	private LoadSettings CreateLoadSettings(LoadTest test)
	{
		var concurrency = test.TestCase.Concurrency;
		var duration = test.TestCase.Duration;
		var interval = test.TestCase.Interval;

		return new LoadSettings
		{
			Concurrency = concurrency,
			Duration = TimeSpan.FromMilliseconds(duration),
			Interval = TimeSpan.FromMilliseconds(interval),
		};
	}

	private LoadExecutionPlan CreateExecutionPlan(LoadTestRunnerContext ctx, LoadSettings settings)
	{
		async Task<TimeSpan> Action() => await base.RunTest(ctx);
		return new LoadExecutionPlan
		{
			Name = ctx.Test.TestDisplayName,
			Action = async () =>
			{
				await Action();
				return true;
			},
			Settings = settings
		};
	}
}

public class LoadTestRunnerContext(
	Specification specification,
	LoadTest test,
	IMessageBus messageBus,
	string? skipReason,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
	TestRunnerContext<LoadTest>(test, messageBus, skipReason, ExplicitOption.Off, aggregator, cancellationTokenSource, test.TestCase.TestMethod.Method, [])
{
	public Specification Specification { get; } = specification;
}
