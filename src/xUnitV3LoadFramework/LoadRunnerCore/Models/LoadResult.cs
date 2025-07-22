namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
	public class LoadResult
	{
		public required string ScenarioName { get; set; }
		public int Total { get; set; }
		public int Success { get; set; }
		public int Failure { get; set; }
		public double Time { get; set; }

		// Latency metrics
		public double MaxLatency { get; set; }
		public double MinLatency { get; set; }
		public double AverageLatency { get; set; }
		public double Percentile95Latency { get; set; }
		public double Percentile99Latency { get; set; }
		public double MedianLatency { get; set; }
		
		// Request tracking metrics
		public int RequestsStarted { get; set; }
		public int RequestsInFlight { get; set; }
		
		// Throughput metrics
		public double RequestsPerSecond { get; set; }
		public double AvgQueueTime { get; set; }
		public double MaxQueueTime { get; set; }
		
		// Resource utilization metrics (for hybrid mode)
		public int WorkerThreadsUsed { get; set; }
		public double WorkerUtilization { get; set; }
		public long PeakMemoryUsage { get; set; }
		public int BatchesCompleted { get; set; }
	}
}