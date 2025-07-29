// Import Akka.NET actor framework for message-driven result aggregation and processing
// Provides the foundation for centralized result collection from distributed workers
using Akka.Actor;
// Import Akka.Event for structured logging within the actor system context
// Enables consistent logging format with actor path and proper severity levels
using Akka.Event;
// Import message contracts for receiving performance data and coordination messages
// Defines all message types used for result reporting and statistical aggregation
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
// Import data models for result structures and load test configuration
// Contains DTOs for aggregated results, metrics, and performance statistics
using xUnitV3LoadFramework.LoadRunnerCore.Models;

// Define namespace for result processing and statistical aggregation actors
namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
	/// <summary>
	/// Actor responsible for collecting, aggregating, and analyzing performance results from load workers.
	/// Centralizes all timing data, success/failure counts, and resource utilization metrics.
	/// Calculates comprehensive statistics including latency percentiles and throughput measurements.
	/// Provides thread-safe result aggregation for distributed load testing scenarios.
	/// </summary>
	public class ResultCollectorActor : ReceiveActor
	{
		// Store the name of the load test scenario for identification and reporting purposes
		// This provides context for all logged messages and result identification
		private readonly string _scenarioName;
		
		// Track the total number of completed test operations (both successful and failed)
		// This represents the actual workload completed during the test execution
		private int _total;
		
		// Count of successful test operations for success rate calculation
		// Used to determine the reliability and stability of the system under test
		private int _success;
		
		// Count of failed test operations for failure rate analysis
		// Critical for identifying system reliability issues and error patterns
		private int _failure;
		
		// Track the total number of requests that have been initiated
		// This may be higher than _total if some requests are still in progress
		private int _started;
		
		// Track the number of currently executing requests for in-flight monitoring
		// Used to calculate accurate throughput and identify request queuing issues
		private int _inFlight;
		
		// Collection of all latency measurements for statistical analysis
		// Used to calculate percentiles, averages, and identify performance patterns
		private readonly List<double> _latencies = new();
		
		// Collection of queue time measurements for worker pool performance analysis
		// Helps identify bottlenecks in work distribution and worker utilization
		private readonly List<double> _queueTimes = new();
		
		// Akka.NET logging adapter for structured logging with actor context
		// Provides consistent logging format with timestamps and severity levels
		private readonly ILoggingAdapter _logger = Context.GetLogger();
		
		// Timestamp when the load test execution began for duration calculation
		// Used as reference point for all timing calculations and throughput metrics
		private DateTime? _startTime = null;
		
		// Track the highest memory usage observed during test execution
		// Important for understanding resource consumption patterns and limits
		private long _peakMemoryUsage = 0;
		
		// Count of completed batches for batch-level performance analysis
		// Helps understand the effectiveness of interval-based load generation
		private int _batchesCompleted = 0;
		
		// Track the maximum number of worker threads utilized during execution
		// Used for worker utilization calculations and resource planning
		private int _workerThreadsUsed = 0;
		
		/// <summary>
		/// Constructor to initialize the ResultCollectorActor with scenario identification.
		/// Sets up all message handlers for various types of performance data and coordination messages.
		/// Establishes the centralized collection point for distributed load test results.
		/// </summary>
		/// <param name="scenarioName">Name identifier for this load test scenario</param>
		public ResultCollectorActor(string scenarioName)
		{
			// Store the scenario name for use in logging and result identification
			// This provides context for all subsequent operations and reporting
			_scenarioName = scenarioName;
			
			// Define message handler for load test start notification
			// This initializes timing and begins the result collection process
			Receive<StartLoadMessage>(_ => 
			{
				// Record the precise start time for accurate duration and throughput calculations
				// This timestamp serves as the reference point for all performance metrics
				_startTime = DateTime.UtcNow;
				
				// Log the test start with scenario name and timestamp for monitoring
				// Provides visibility into test lifecycle and timing correlation
				_logger.Info("Load scenario '{0}' started at {1}", _scenarioName, _startTime);
			});

			// Define message handler for request initiation notifications
			// Tracks when new requests begin execution for accurate in-flight monitoring
			Receive<RequestStartedMessage>(_ =>
			{
				// Increment counter for total started requests
				// This tracks the actual workload initiated regardless of completion status
				_started++;
				
				// Increment in-flight counter to track concurrent request execution
				// Used for throughput calculations and system capacity analysis
				_inFlight++;
				
				// Capture current memory usage and update peak if necessary
				// This helps identify memory consumption patterns and potential leaks
				var currentMemory = GC.GetTotalMemory(false);
				if (currentMemory > _peakMemoryUsage)
					_peakMemoryUsage = currentMemory;
				
				// Log detailed request tracking information for debugging and monitoring
				// Provides real-time visibility into request flow and concurrency levels
				_logger.Debug("Request started. Total started: {0}, In-flight: {1}", _started, _inFlight);
			});

			// Define message handler for individual test result processing
			// This is the core aggregation point for all performance data
			Receive<StepResultMessage>(msg =>
			{
				// Increment total completed operations counter
				// This represents actual work completed during the test
				_total++;
				
				// Decrement in-flight counter as this request has completed
				// Maintains accurate tracking of concurrent execution levels
				_inFlight--;
				
				// Update success or failure counters based on result
				// Critical for calculating success rates and system reliability metrics
				if (msg.IsSuccess)
					_success++;
				else
					_failure++;

				// Add latency measurement to collection for statistical analysis
				// This data is used for percentile calculations and performance trending
				_latencies.Add(msg.Latency);
				
				// Add queue time measurement if provided (hybrid mode feature)
				// Queue time helps analyze worker pool efficiency and capacity planning
				if (msg.QueueTime > 0)
					_queueTimes.Add(msg.QueueTime);

				// Log detailed completion statistics for real-time monitoring
				// Provides immediate feedback on test progress and result distribution
				_logger.Debug("Step completed. Success: {0}, Failure: {1}, In-flight: {2}", 
					_success, _failure, _inFlight);
			});

			// Define message handler for batch completion notifications
			// Used to track interval-based execution progress and batch-level metrics
			Receive<BatchCompletedMessage>(_ =>
			{
				// Increment counter for completed batches
				// This helps understand the effectiveness of interval-based load generation
				_batchesCompleted++;
				
				// Log batch completion for monitoring execution progress
				// Provides visibility into the batch processing pattern
				_logger.Debug("Batch completed. Total batches: {0}", _batchesCompleted);
			});

			// Define message handler for worker thread count updates
			// Tracks the maximum worker thread utilization for resource analysis
			Receive<WorkerThreadCountMessage>(msg =>
			{
				// Update worker thread count if this is higher than previously recorded
				// This captures the peak worker utilization during test execution
				if (msg.ThreadCount > _workerThreadsUsed)
				{
					// Store the new maximum worker thread count
					_workerThreadsUsed = msg.ThreadCount;
					
					// Log the worker thread count update for monitoring
					// Helps understand resource scaling and utilization patterns
					_logger.Debug("Worker thread count updated: {0}", _workerThreadsUsed);
				}
			});

			// Define message handler for final result aggregation and reporting
			// This calculates all statistics and returns comprehensive performance metrics
			Receive<GetLoadResultMessage>(_ =>
			{
				// Capture the end time for accurate duration calculation
				// This provides the precise test execution window
				var endTime = DateTime.UtcNow;
				
				// Calculate total test duration in seconds for throughput calculations
				// Handle case where start time might not be set for defensive programming
				var totalTimeSec = (_startTime.HasValue) ? (endTime - _startTime.Value).TotalSeconds : 0;
				
				// Create comprehensive result object with all collected metrics
				// This aggregates all performance data into a single reportable structure
				var result = new LoadResult
				{
					// Basic identification and execution metrics
					ScenarioName = _scenarioName,
					Total = _total,
					Success = _success,
					Failure = _failure,
					
					// Latency statistics calculated from all collected measurements
					MaxLatency = _latencies.Any() ? _latencies.Max() : 0,
					MinLatency = _latencies.Any() ? _latencies.Min() : 0,
					AverageLatency = _latencies.Any() ? _latencies.Average() : 0,
					Percentile95Latency = _latencies.Any() ? CalculatePercentile(_latencies, 95) : 0,
					Percentile99Latency = _latencies.Any() ? CalculatePercentile(_latencies, 99) : 0,
					MedianLatency = _latencies.Any() ? CalculatePercentile(_latencies, 50) : 0,
					
					// Request flow and concurrency tracking metrics
					RequestsStarted = _started,
					RequestsInFlight = _inFlight,
					
					// Throughput and performance metrics
					RequestsPerSecond = totalTimeSec > 0 ? _total / totalTimeSec : 0,
					AvgQueueTime = _queueTimes.Any() ? _queueTimes.Average() : 0,
					MaxQueueTime = _queueTimes.Any() ? _queueTimes.Max() : 0,
					
					// Resource utilization and efficiency metrics
					WorkerThreadsUsed = _workerThreadsUsed,
					WorkerUtilization = _workerThreadsUsed > 0 && totalTimeSec > 0 ? (_started / (double)_workerThreadsUsed) / totalTimeSec : 0,
					PeakMemoryUsage = _peakMemoryUsage,
					BatchesCompleted = _batchesCompleted,
					
					// Overall test execution time
					Time = totalTimeSec
				};

				// Log comprehensive completion summary with key metrics
				// Provides immediate visibility into final test results and any outstanding requests
				_logger.Info("Scenario '{0}' completed. Started: {1}, Completed: {2}, In-flight: {3}", 
					_scenarioName, _started, _total, _inFlight);

				// Send the aggregated result back to the requesting actor
				// This completes the result collection and analysis process
				Sender.Tell(result);
			});
		}

		/// <summary>
		/// Calculates the specified percentile from a collection of latency measurements.
		/// Uses the ceiling method for percentile calculation to provide conservative estimates.
		/// Handles edge cases and ensures robust statistical analysis of performance data.
		/// </summary>
		/// <param name="latencies">Collection of latency measurements to analyze</param>
		/// <param name="percentile">Target percentile value (0-100) to calculate</param>
		/// <returns>The latency value at the specified percentile</returns>
		private static double CalculatePercentile(List<double> latencies, double percentile)
		{
			// Sort the latency collection to enable percentile calculation
			// This is required for proper statistical analysis of ordered data
			latencies.Sort();
			
			// Calculate the index for the specified percentile using ceiling method
			// Ceiling method provides conservative estimates by rounding up
			var index = (int)System.Math.Ceiling((percentile / 100.0) * latencies.Count) - 1;
			
			// Return the latency value at the calculated index with bounds checking
			// Min operation prevents index out of bounds for edge cases
			return latencies[System.Math.Min(index, latencies.Count - 1)];
		}
	}
}