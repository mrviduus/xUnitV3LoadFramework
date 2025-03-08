using System.Diagnostics;
using LoadRunnerCore;
using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;

var scenario = new LoadExecutionPlan()
{
    Name = "SimpleScenario",
    Settings = new LoadSettings()
    {
        Concurrency = 2,
        Duration = TimeSpan.FromSeconds(1),
        Interval = TimeSpan.FromSeconds(1)
    },
    Action = async () =>
    {
        Console.WriteLine("Action started");
        return true;
    }
};

var stopwatch = Stopwatch.StartNew();

var result = await LoadRunner.Run(scenario);
stopwatch.Stop();

Console.WriteLine($"Total time: {stopwatch.Elapsed}");
Console.WriteLine($"Total: {result.Total}");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Failure: {result.Failure}");