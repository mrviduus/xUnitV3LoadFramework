using System;
using System.Threading.Tasks;
using LoadRunnerCore.Models;
using Xunit;
using LoadRunnerCore.Runner;

namespace LoadRunnerTests
{
    public class LoadRunnerTest
    {
        [Fact]
        public async Task RunPlan_WithNullSteps_ThrowsArgumentNullException()
        {
            var plan = new LoadPlan
            {
                Steps = null,
                Settings = new LoadSettings { Concurrency = 1, Duration = TimeSpan.FromSeconds(1) }
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => LoadRunner.Run(plan));
        }

        [Fact]
        public async Task RunPlan_WithZeroStepsOrConcurrency_ReturnsEmptyLoadResult()
        {
            var plan = new LoadPlan
            {
                Name = "TestPlan",
                Steps = Array.Empty<LoadStep>(),
                Settings = new LoadSettings { Concurrency = 0, Duration = TimeSpan.FromSeconds(1) }
            };

            var result = await LoadRunner.Run(plan);
            Assert.Equal("TestPlan", result.ScenarioName);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public async Task RunPlan_WithValidStepsAndConcurrency_ReturnsLoadResult()
        {
            var plan = new LoadPlan
            {
                Name = "ConcurrentTestPlan",
                Steps = new LoadStep[]
                {
                    new LoadStep { Name = "Step1" },
                    new LoadStep { Name = "Step2" }
                },
                Settings = new LoadSettings { Concurrency = 2, Duration = TimeSpan.FromSeconds(1) }
            };

            var result = await LoadRunner.Run(plan);
            Assert.Equal("ConcurrentTestPlan", result.ScenarioName);
            Assert.True(result.Total > 0);
        }
    }
}