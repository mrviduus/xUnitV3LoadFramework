namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadSettings
    {
        public int Concurrency { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }
    }
}