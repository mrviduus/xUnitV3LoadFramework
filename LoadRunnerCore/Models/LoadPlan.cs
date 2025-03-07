namespace LoadRunnerCore.Models
{
    public class LoadPlan
    {
        public string Name { get; set; }
        public LoadStep[] Steps { get; set; }
        public LoadSettings Settings { get; set; }
    }
}