using System.Diagnostics;
using LoadTestRunner.Models;

var steps = new LoadTestStep[]
{
    new LoadTestStep()
    {
        Name = "Step1",
        Action = () =>
        {
            Console.WriteLine("Step1");
            Console.WriteLine("-----------------");
            return Task.FromResult(true);
        }
    },
    new LoadTestStep()
    {
        Name = "Step2",
        Action =  () =>
        {
            Console.WriteLine("Step2");
            Console.WriteLine("-----------------");
            return Task.FromResult(true);
        }
    }
};

var scenario = new LoadTestPlan()
{
    Name = "SimpleScenario",
    Steps = steps,
    Settings = new LoadExecutionSettings()
    {
        Concurrency = 2,
        Duration = TimeSpan.FromSeconds(1),
        Interval = TimeSpan.FromMilliseconds(100)
    }
};

var stopwatch = Stopwatch.StartNew();
var result = await scenario.Run();
stopwatch.Stop();

Console.WriteLine($"Total time: {stopwatch.Elapsed}");
Console.WriteLine($"Total: {result.Total}");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Failure: {result.Failure}");