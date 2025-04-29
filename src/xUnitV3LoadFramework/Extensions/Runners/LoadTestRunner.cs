using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Core.Models;
using xUnitV3LoadFramework.Core.Runner;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Runners;

public class LoadTestRunner :
	TestRunner<LoadTestRunnerContext, LoadTest>
{
	public static LoadTestRunner Instance { get; } = new();

	// We don't want to claim to create or dispose the object here, because we share an already created
	// instance among all the tests, and we don't want to dispatch the messages related to creation
	// and disposal either. So we return false for the creation/disposal options.
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
		await ctxt.InitializeAsync();

		// Queue TestStarting explicitly to populate metadata
		await OnTestStarting(ctxt);

		var summary = new RunSummary { Total = 1 };
		var elapsedTime = TimeSpan.Zero;

		var loadSettings = CreateLoadSettings(test);
		if (loadSettings == null)
		{
			summary.NotRun = 1;
			await OnTestNotRun(ctxt, "", null);
		}
		else
		{
			var executionPlan = CreateExecutionPlan(ctxt, loadSettings);
			var loadResult = await LoadRunner.Run(executionPlan);
			elapsedTime = loadSettings.Duration;
			summary.Time = (decimal)elapsedTime.TotalSeconds;

			if (loadResult.Failure > 0)
			{
				summary.Failed = loadResult.Failure;
				var exception = new Exception($"{loadResult.Failure} load test(s) failed.");
				await OnTestFailed(ctxt, exception, summary.Time, "", null);
			}
			else
			{
				await OnTestPassed(ctxt, summary.Time, "", null);
			}
		}

		// IMPORTANT: Queue TestFinished explicitly to ensure test metadata is completed
		await OnTestFinished(ctxt, summary.Time, "", null, null);

		return summary;
	}


	private LoadSettings CreateLoadSettings(LoadTest test)
	{
		var concurrency = test.TestCase.Concurrency;
		var duration = test.TestCase.Duration;
		var interval = test.TestCase.Interval;

		if (concurrency <= 0)
			return null;

		if (duration <= 0)
			return null;

		if (interval <= 0)
			return null;

		return new LoadSettings
		{
			Concurrency = concurrency,
			Duration = TimeSpan.FromMilliseconds(duration),
			Interval = TimeSpan.FromMilliseconds(interval),
		};
	}

	private LoadExecutionPlan CreateExecutionPlan(LoadTestRunnerContext ctx, LoadSettings settings)
	{
		var action = async () => await base.RunTest(ctx);
		return new LoadExecutionPlan
		{
			Name = ctx.Test.TestDisplayName,
			Action = async () =>
			{
				await action();
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