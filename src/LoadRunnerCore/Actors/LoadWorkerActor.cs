using Akka.Actor;
using Akka.Event;
using LoadRunnerCore.Messages;
using LoadRunnerCore.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoadRunnerCore.Actors
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
				_logger.Info("LoadWorkerActor '{0}' started load test.", workerName);

				while (!cts.Token.IsCancellationRequested)
				{
					var tasks = Enumerable.Range(0, _executionPlan.Settings.Concurrency)
						.Select(_ => Task.Run(async () =>
						{
							var stopwatch = Stopwatch.StartNew();
							bool result = await _executionPlan.Action();
							stopwatch.Stop();
							var latency = stopwatch.Elapsed.TotalMilliseconds;

							_resultCollector.Tell(new StepResultMessage(result, latency));

							_logger.Debug("[{0}] Result: {1}, Latency: {2:F2} ms", workerName, result, latency);
						}, cts.Token))
						.ToArray();

					await Task.WhenAll(tasks);

					if (!cts.Token.IsCancellationRequested)
					{
						await Task.Delay(_executionPlan.Settings.Interval, cts.Token);
					}
				}
			}
			catch (TaskCanceledException)
			{
				_logger.Warning("LoadWorkerActor '{0}' load test canceled due to duration expiration.", workerName);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "LoadWorkerActor '{0}' encountered an unexpected error.", workerName);
			}
			finally
			{
				_logger.Info("LoadWorkerActor '{0}' has completed load testing.", workerName);

				// Clearly collect final result from ResultCollectorActor
				var finalResult = await _resultCollector.Ask<LoadResult>(
					new GetLoadResultMessage(), TimeSpan.FromSeconds(5));

				// Explicitly reply to the sender to avoid timeout!
				Sender.Tell(finalResult);
			}
		}
	}
}