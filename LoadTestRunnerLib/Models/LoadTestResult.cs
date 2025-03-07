namespace xUnitLoadRunnerLib.Models
{
    public class LoadTestResult
    {
        public string ScenarioName { get; set; }
        public int Total { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
    }
}