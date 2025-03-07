using xUnitLoadRunnerLib;

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
            return Task.FromResult(false);
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
        Duration = TimeSpan.FromSeconds(2),
        Interval = TimeSpan.FromSeconds(1)
    }

};

var result = await scenario.Run();
Console.WriteLine($"total: {result.Total}");
Console.WriteLine($"success: {result.Success}");
Console.WriteLine($"failure: {result.Failure}");