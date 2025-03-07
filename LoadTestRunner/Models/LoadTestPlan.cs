using System.Threading.Tasks;

namespace LoadTestRunner.Models
{
    public class LoadTestPlan
    {
        public string Name { get; set; }
        public LoadTestStep[] Steps { get; set; }
        public LoadExecutionSettings Settings { get; set; }

        public Task<LoadTestResult> Run()
        {
            return LoadTestRunner.Run(this);
        }
    }
}