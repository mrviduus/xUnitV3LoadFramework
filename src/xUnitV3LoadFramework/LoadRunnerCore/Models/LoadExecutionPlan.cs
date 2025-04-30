namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadExecutionPlan
    {
        public string Name { get; set; }
        public LoadSettings Settings { get; set; }
        public Func<Task<bool>> Action { get; set; }
    }
}