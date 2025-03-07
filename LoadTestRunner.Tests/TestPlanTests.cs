using System;
using System.Threading.Tasks;
using Xunit;
using xUnitLoadRunnerLib;

namespace xUnitLoadRunner.Tests
{
    public class LoadTestRunnerLoadTests
    {
        [Fact]
        public async Task Run_ReturnsCorrectReport_WhenStepsAreSuccessful()
        {
            var steps = new LoadTestStep[]
            {
                new LoadTestStep { Action = () => Task.FromResult(true) },
                new LoadTestStep { Action = () => Task.FromResult(true) }
            };
            var settings = new LoadExecutionSettings
            {
                Concurrency = 1,
                Duration = TimeSpan.FromSeconds(10),
                Interval = TimeSpan.FromSeconds(1)
            };
            var testPlan = new LoadTestPlan { Name = "TestPlan1", Steps = steps, Settings = settings };

            var result = await LoadTestRunner.Run(testPlan);
            
            Assert.NotNull(result);
            Assert.Equal("TestPlan1", result.ScenarioName);
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Success);
            Assert.Equal(0, result.Failure);
        }

        [Fact]
        public async Task Run_ReturnsCorrectReport_WhenStepsFail()
        {
            var steps = new LoadTestStep[]
            {
                new LoadTestStep { Action = () => Task.FromResult(false) },
                new LoadTestStep { Action = () => Task.FromResult(false) }
            };
            var settings = new LoadExecutionSettings
            {
                Concurrency = 2,
                Duration = TimeSpan.FromSeconds(1),
                Interval = TimeSpan.FromSeconds(1)
            };
            var testPlan = new LoadTestPlan { Name = "TestPlan2", Steps = steps, Settings = settings };

            var result = await LoadTestRunner.Run(testPlan);

            Assert.NotNull(result);
            Assert.Equal("TestPlan2", result.ScenarioName);
            Assert.Equal(2, result.Total);
            Assert.Equal(0, result.Success);
            Assert.Equal(2, result.Failure);
        }

        [Fact]
        public async Task Run_ThrowsArgumentNullException_WhenStepsAreNull()
        {
            var settings = new LoadExecutionSettings
            {
                Concurrency = 2,
                Duration = TimeSpan.FromSeconds(1),
                Interval = TimeSpan.FromMilliseconds(100)
            };
            var testPlan = new LoadTestPlan { Name = "TestPlan3", Steps = null, Settings = settings };

            await Assert.ThrowsAsync<ArgumentNullException>(() => LoadTestRunner.Run(testPlan));
        }

        [Fact]
        public async Task Run_ReturnsEmptyReport_WhenStepsAreEmpty()
        {
            var steps = new LoadTestStep[] { };
            var settings = new LoadExecutionSettings
            {
                Concurrency = 2,
                Duration = TimeSpan.FromSeconds(1),
                Interval = TimeSpan.FromMilliseconds(100)
            };
            var testPlan = new LoadTestPlan { Name = "TestPlan4", Steps = steps, Settings = settings };

            var result = await LoadTestRunner.Run(testPlan);

            Assert.Equal("TestPlan4", result.ScenarioName);
            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.Success);
            Assert.Equal(0, result.Failure);
        }

        [Fact]
        public async Task Run_ReturnsEmptyReport_WhenConcurrencyIsZero()
        {
            var steps = new LoadTestStep[]
            {
                new LoadTestStep { Action = () => Task.FromResult(true) }
            };
            var settings = new LoadExecutionSettings
            {
                Concurrency = 0,
                Duration = TimeSpan.FromSeconds(1),
                Interval = TimeSpan.FromMilliseconds(100)
            };
            var testPlan = new LoadTestPlan { Name = "TestPlan5", Steps = steps, Settings = settings };

            var result = await LoadTestRunner.Run(testPlan);

            Assert.Equal("TestPlan5", result.ScenarioName);
            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.Success);
            Assert.Equal(0, result.Failure);
        }
    }
}