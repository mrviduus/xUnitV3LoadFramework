using Akka.Actor;
using Akka.Event;
using xUnitV3LoadFramework.Core.Messages;
using xUnitV3LoadFramework.Core.Models;

namespace xUnitV3LoadFramework.Core.Actors
{
	public class LoadWorkerActor : ReceiveActor
	{
		private readonly LoadExecutionPlan _executionPlan;
		private readonly IActorRef _resultCollector;
		private readonly ILoggingAdapter _logger = Context.GetLogger();

		public LoadWorkerActor(LoadExecutionPlan executionPlan, IActorRef resultCollector)
		{
			_executionPlan = executionPlan;
			_resultCollector = resultCollector;
			ReceiveAsync<StartLoadMessage>(async _ => await RunWorkAsync());
		}

		private async Task RunWorkAsync()
		{
			var workerName = Self.Path.Name;
			using var cts = new CancellationTokenSource(_executionPlan.Settings.Duration);

			try
			{
				_logger.Info("Worker {0} started load test in parallel mode.", workerName);

				// Keep running until canceled or duration expires
				while (!cts.Token.IsCancellationRequested)
				{
					// Use Settings.Concurrency to run multiple tasks in parallel
					var tasks = Enumerable.Range(0, _executionPlan.Settings.Concurrency)
						.Select(_ => Task.Run(async () =>
						{
							var result = await _executionPlan.Action();
							_resultCollector.Tell(new StepResultMessage(result));
							_logger.Debug("Worker {0} step result: {1}", workerName, result);
						}, cts.Token))
						.ToArray();

					await Task.WhenAll(tasks);

					// Wait for the configured interval before the next round, unless canceled
					if (!cts.Token.IsCancellationRequested)
					{
						await Task.Delay(_executionPlan.Settings.Interval, cts.Token);
					}
				}
			}
			catch (TaskCanceledException)
			{
				_logger.Info("Worker {0} canceled during load test execution.", workerName);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Worker {0} encountered an error.", workerName);
			}
			finally
			{
				_logger.Info("Worker {0} returning final result.", workerName);
				Sender.Tell(new LoadResult());
			}
		}
	}
}