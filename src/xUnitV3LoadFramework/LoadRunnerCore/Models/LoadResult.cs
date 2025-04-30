namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
	public class LoadResult
	{
		public string ScenarioName { get; set; } = string.Empty;
		public int Total { get; set; }
		public int Success { get; set; }
		public int Failure { get; set; }
		public decimal Time { get; set; }

		// New metrics
		public double MaxLatency { get; set; }
		public double MinLatency { get; set; }
		public double AverageLatency { get; set; }
		public double Percentile95Latency { get; set; }
	}
}