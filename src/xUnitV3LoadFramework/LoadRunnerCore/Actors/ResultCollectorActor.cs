using Akka.Actor;
using Akka.Event;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
	public class ResultCollectorActor : ReceiveActor
	{
		private readonly string _scenarioName;
		private int _total;
		private int _success;
		private int _failure;
		private int _started; // Track requests that have started
		private int _inFlight; // Track currently running requests
		private readonly List<double> _latencies = new();
		private readonly List<double> _queueTimes = new();
		private readonly ILoggingAdapter _logger = Context.GetLogger();
		
		private DateTime? _startTime = null;
		private long _peakMemoryUsage = 0;
		private int _batchesCompleted = 0;
		private int _workerThreadsUsed = 0;
		
		public ResultCollectorActor(string scenarioName)
		{
			_scenarioName = scenarioName;
			
			Receive<StartLoadMessage>(_ => 
			{
				_startTime = DateTime.UtcNow;
				_logger.Info("Load scenario '{0}' started at {1}", _scenarioName, _startTime);
			});

			Receive<RequestStartedMessage>(_ =>
			{
				_started++;
				_inFlight++;
				
				// Track peak memory usage
				var currentMemory = GC.GetTotalMemory(false);
				if (currentMemory > _peakMemoryUsage)
					_peakMemoryUsage = currentMemory;
				
				_logger.Debug("Request started. Total started: {0}, In-flight: {1}", _started, _inFlight);
			});

			Receive<StepResultMessage>(msg =>
			{
				_total++;
				_inFlight--;
				if (msg.IsSuccess)
					_success++;
				else
					_failure++;

				_latencies.Add(msg.Latency);
				
				// Track queue time if provided
				if (msg.QueueTime > 0)
					_queueTimes.Add(msg.QueueTime);

				_logger.Debug("Step completed. Success: {0}, Failure: {1}, In-flight: {2}", 
					_success, _failure, _inFlight);
			});

			// Handle batch completion notifications
			Receive<BatchCompletedMessage>(_ =>
			{
				_batchesCompleted++;
				_logger.Debug("Batch completed. Total batches: {0}", _batchesCompleted);
			});

			// Handle worker thread count updates
			Receive<WorkerThreadCountMessage>(msg =>
			{
				if (msg.ThreadCount > _workerThreadsUsed)
				{
					_workerThreadsUsed = msg.ThreadCount;
					_logger.Debug("Worker thread count updated: {0}", _workerThreadsUsed);
				}
			});

			Receive<GetLoadResultMessage>(_ =>
			{
				var endTime = DateTime.UtcNow;
				var totalTimeSec = (_startTime.HasValue) ? (endTime - _startTime.Value).TotalSeconds : 0;
				var result = new LoadResult
				{
					ScenarioName = _scenarioName,
					Total = _total,
					Success = _success,
					Failure = _failure,
					MaxLatency = _latencies.Any() ? _latencies.Max() : 0,
					MinLatency = _latencies.Any() ? _latencies.Min() : 0,
					AverageLatency = _latencies.Any() ? _latencies.Average() : 0,
					Percentile95Latency = _latencies.Any() ? CalculatePercentile(_latencies, 95) : 0,
					Percentile99Latency = _latencies.Any() ? CalculatePercentile(_latencies, 99) : 0,
					MedianLatency = _latencies.Any() ? CalculatePercentile(_latencies, 50) : 0,
					// Request tracking metrics
					RequestsStarted = _started,
					RequestsInFlight = _inFlight,
					// Throughput metrics
					RequestsPerSecond = totalTimeSec > 0 ? _total / totalTimeSec : 0,
					AvgQueueTime = _queueTimes.Any() ? _queueTimes.Average() : 0,
					MaxQueueTime = _queueTimes.Any() ? _queueTimes.Max() : 0,
					// Resource utilization metrics
					WorkerThreadsUsed = _workerThreadsUsed,
					WorkerUtilization = _workerThreadsUsed > 0 && totalTimeSec > 0 ? (_started / (double)_workerThreadsUsed) / totalTimeSec : 0,
					PeakMemoryUsage = _peakMemoryUsage,
					BatchesCompleted = _batchesCompleted,
					// Total time taken for the test
					Time = totalTimeSec
				};

				_logger.Info("Scenario '{0}' completed. Started: {1}, Completed: {2}, In-flight: {3}", 
					_scenarioName, _started, _total, _inFlight);

				Sender.Tell(result);
			});
		}

		private static double CalculatePercentile(List<double> latencies, double percentile)
		{
			latencies.Sort();
			var index = (int)System.Math.Ceiling((percentile / 100.0) * latencies.Count) - 1;
			return latencies[System.Math.Min(index, latencies.Count - 1)];
		}
	}
}