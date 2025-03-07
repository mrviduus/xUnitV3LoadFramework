using System.Diagnostics;
using LoadRunnerCore;
using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;

var steps = new LoadStep[]
{
    new LoadStep()
    {
        Name = "Step1",
        Action = () =>
        {
            Console.WriteLine("Step1");
            Console.WriteLine("-----------------");
            return Task.FromResult(true);
        }
    },
    new LoadStep()
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

var scenario = new LoadPlan()
{
    Name = "SimpleScenario",
    Steps = steps,
    Settings = new LoadSettings()
    {
        Concurrency = 2,
        Duration = TimeSpan.FromSeconds(1),
        Interval = TimeSpan.FromMilliseconds(100)
    }
};

var stopwatch = Stopwatch.StartNew();

var result = await LoadRunner.Run(scenario);
stopwatch.Stop();

Console.WriteLine($"Total time: {stopwatch.Elapsed}");
Console.WriteLine($"Total: {result.Total}");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Failure: {result.Failure}");