namespace xUnitV3LoadFramework.Core.Models
{
	public class LoadResult
	{
		public string ScenarioName { get; set; }
		public int Total { get; set; }
		public int Success { get; set; }
		public int Failure { get; set; }
		public decimal Time { get; set; }
	}
}