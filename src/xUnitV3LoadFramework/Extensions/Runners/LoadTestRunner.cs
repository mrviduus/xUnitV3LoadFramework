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
		var reportGenerator = new LoadTestResultReport(ctxt, result);
		var summaryMessage = reportGenerator.GenerateSummaryMessage();

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
