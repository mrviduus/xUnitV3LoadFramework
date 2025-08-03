namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
	/// <summary>
	/// Comprehensive load test results containing performance metrics and statistics.
	/// Provides detailed analysis of load test execution including timing, throughput, and resource utilization.
	/// </summary>
	public class LoadResult
	{
		/// <summary>
		/// Gets or sets the name of the load test scenario that was executed.
		/// Used for identification and reporting purposes across multiple test runs.
		/// </summary>
		public required string ScenarioName { get; set; }
		
		/// <summary>
		/// Gets or sets the descriptive name for this load test result.
		/// Provides human-readable identification for fluent API usage and reporting.
		/// Alias for ScenarioName for fluent API compatibility.
		/// </summary>
		public string Name 
		{ 
			get => ScenarioName; 
			set => ScenarioName = value; 
		}
		
		/// <summary>
		/// Gets or sets the total number of operations attempted during the load test.
		/// Represents the sum of successful and failed operations.
		/// </summary>
		public int Total { get; set; }
		
		/// <summary>
		/// Gets or sets the number of operations that completed successfully.
		/// Used to calculate success rate and overall test reliability.
		/// </summary>
		public int Success { get; set; }
		
		/// <summary>
		/// Gets or sets the number of operations that failed during execution.
		/// Indicates potential system issues or capacity limits under load.
		/// </summary>
		public int Failure { get; set; }
		
		/// <summary>
		/// Gets or sets the total execution time for the load test in seconds.
		/// Includes setup, execution, and teardown phases of the test.
		/// </summary>
		public double Time { get; set; }

		// Latency metrics section - provides detailed response time analysis
		
		/// <summary>
		/// Gets or sets the maximum response latency observed during the test in milliseconds.
		/// Indicates worst-case performance under load conditions.
		/// </summary>
		public double MaxLatency { get; set; }
		
		/// <summary>
		/// Gets or sets the minimum response latency observed during the test in milliseconds.
		/// Represents best-case performance when system is least loaded.
		/// </summary>
		public double MinLatency { get; set; }
		
		/// <summary>
		/// Gets or sets the average response latency across all operations in milliseconds.
		/// Provides a general indication of system performance under load.
		/// </summary>
		public double AverageLatency { get; set; }
		
		/// <summary>
		/// Gets or sets the 95th percentile latency in milliseconds.
		/// Indicates that 95% of requests completed within this time frame.
		/// </summary>
		public double Percentile95Latency { get; set; }
		
		/// <summary>
		/// Gets or sets the 99th percentile latency in milliseconds.
		/// Shows the latency threshold for 99% of all requests, highlighting outliers.
		/// </summary>
		public double Percentile99Latency { get; set; }
		
		/// <summary>
		/// Gets or sets the median (50th percentile) latency in milliseconds.
		/// Represents the middle value when all latencies are sorted, less affected by outliers.
		/// </summary>
		public double MedianLatency { get; set; }
		
		// Request tracking metrics section - monitors test execution flow
		
		/// <summary>
		/// Gets or sets the total number of requests that have been initiated.
		/// Tracks how many operations have begun execution during the test.
		/// </summary>
		public int RequestsStarted { get; set; }
		
		/// <summary>
		/// Gets or sets the number of requests currently being processed.
		/// Indicates concurrent load at the time of measurement.
		/// </summary>
		public int RequestsInFlight { get; set; }
		
		// Throughput metrics section - measures system capacity and performance
		
		/// <summary>
		/// Gets or sets the calculated requests per second throughput.
		/// Primary metric for measuring system capacity under load.
		/// </summary>
		public double RequestsPerSecond { get; set; }
		
		/// <summary>
		/// Gets or sets the average time requests spent waiting in queue before execution.
		/// Indicates system saturation and resource contention levels.
		/// </summary>
		public double AvgQueueTime { get; set; }
		
		/// <summary>
		/// Gets or sets the maximum time any request spent waiting in queue.
		/// Shows worst-case queueing delays under peak load conditions.
		/// </summary>
		public double MaxQueueTime { get; set; }
		
		// Resource utilization metrics section - for hybrid mode performance analysis
		
		/// <summary>
		/// Gets or sets the number of worker threads utilized during hybrid mode execution.
		/// Helps optimize thread pool sizing for future test runs.
		/// </summary>
		public int WorkerThreadsUsed { get; set; }
		
		/// <summary>
		/// Gets or sets the percentage of worker thread utilization (0.0 to 1.0).
		/// Indicates efficiency of resource usage in hybrid execution mode.
		/// </summary>
		public double WorkerUtilization { get; set; }
		
		/// <summary>
		/// Gets or sets the peak memory usage during test execution in bytes.
		/// Monitors memory consumption patterns and potential memory leaks.
		/// </summary>
		public long PeakMemoryUsage { get; set; }
		
		/// <summary>
		/// Gets or sets the total number of execution batches completed during the test.
		/// Tracks the granular execution pattern and batch processing efficiency.
		/// </summary>
		public int BatchesCompleted { get; set; }
	}
}