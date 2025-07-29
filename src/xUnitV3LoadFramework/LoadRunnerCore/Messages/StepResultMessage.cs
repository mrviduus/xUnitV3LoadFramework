// Define namespace for actor communication messages and performance data contracts
// Contains all message types used for reporting test results and metrics
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
	/// <summary>
	/// Message containing the execution result of a single load test operation.
	/// Sent from load worker actors to result collector for comprehensive performance tracking.
	/// This message carries all essential timing and success data for statistical analysis.
	/// Enables centralized aggregation of performance metrics from distributed workers.
	/// Critical for calculating percentiles, throughput, and system reliability statistics.
	/// </summary>
	public class StepResultMessage
	{
		/// <summary>
		/// Gets a value indicating whether the load test operation completed successfully.
		/// Used to calculate success rates and identify potential system issues.
		/// True indicates the operation met all success criteria and completed without errors.
		/// False indicates failure, timeout, exception, or other unsuccessful completion.
		/// </summary>
		public bool IsSuccess { get; }
		
		/// <summary>
		/// Gets the execution latency of the operation in milliseconds.
		/// Measures the time from operation start to completion for performance analysis.
		/// This timing excludes queue time and represents pure execution duration.
		/// Used for calculating percentiles, averages, and identifying performance trends.
		/// Critical metric for understanding system response characteristics under load.
		/// </summary>
		public double Latency { get; }
		
		/// <summary>
		/// Gets the time the operation spent waiting in queue before execution in milliseconds.
		/// Indicates system saturation and resource contention levels.
		/// High queue times suggest worker pool undersizing or system bottlenecks.
		/// Zero queue time indicates immediate processing without resource constraints.
		/// Only meaningful in hybrid mode with channel-based work distribution.
		/// </summary>
		public double QueueTime { get; }

		/// <summary>
		/// Initializes a new instance of the StepResultMessage class with comprehensive timing data.
		/// Creates a complete result record for aggregation and statistical analysis.
		/// This constructor captures all essential metrics for performance evaluation.
		/// </summary>
		/// <param name="isSuccess">True if the operation completed successfully, false otherwise</param>
		/// <param name="latency">The execution latency in milliseconds for performance analysis</param>
		/// <param name="queueTime">The queue waiting time in milliseconds (default: 0 for task-based mode)</param>
		public StepResultMessage(bool isSuccess, double latency, double queueTime = 0)
		{
			// Store the success status for result aggregation and reliability calculations
			// This determines whether the operation counts toward success rate metrics
			IsSuccess = isSuccess;
			
			// Record the execution latency for comprehensive performance metrics
			// This timing data is essential for percentile calculations and trend analysis
			Latency = latency;
			
			// Track queue time for resource utilization analysis and capacity planning
			// Helps identify worker pool efficiency and potential system bottlenecks
			QueueTime = queueTime;
		}
	}
}