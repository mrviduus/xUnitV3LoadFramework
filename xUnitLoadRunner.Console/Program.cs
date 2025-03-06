using xUnitLoadRunnerLib;

var steps = new TestStep[]
{
    new TestStep()
    {
        Name = "Step1",
        Action = async () =>
        {
            Console.WriteLine("Step1");
            return true;
        }
    }
};

var scenario = new TestPlan
{
    Name = "SimpleScenario",
    Steps = steps,
    Concurrency = 1,
    Duration = TimeSpan.FromSeconds(10),
    Interval = TimeSpan.FromSeconds(1)
};

await scenario.Run();