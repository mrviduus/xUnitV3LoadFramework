using System.Diagnostics;
using xUnitV3LoadFramework.LoadRunnerCore.Configuration;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

Console.WriteLine("Testing Hybrid LoadWorkerActor Implementation");
Console.WriteLine("==============================================");

await TestTaskBasedVsHybrid();