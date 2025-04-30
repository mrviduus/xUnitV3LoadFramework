namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadExecutionPlan
    {
        public required string Name { get; set; }
        public required LoadSettings Settings { get; set; }
        public required Func<Task<bool>> Action { get; set; }
    }
}