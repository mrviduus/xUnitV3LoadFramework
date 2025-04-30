using System.Diagnostics;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;


var scenario = new LoadExecutionPlan()
{
    Name = "SimpleScenario",
    Settings = new LoadSettings()
    {
        Concurrency = 2,
        Duration = TimeSpan.FromSeconds(1),
        Interval = TimeSpan.FromSeconds(1)
    },
    Action = static async () =>
    {
        Console.WriteLine("Action started");
        return await Task.FromResult(true);
    }
};

var stopwatch = Stopwatch.StartNew();

var result = await LoadRunner.Run(scenario);
stopwatch.Stop();

Console.WriteLine($"Total time: {stopwatch.Elapsed}");
Console.WriteLine($"Total: {result.Total}");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Failure: {result.Failure}");