using System;
using System.Threading.Tasks;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Test
{
    public class TimeoutFixTest
    {
        [Load(concurrency: 1, duration: 1000, interval: 100)]
        public async Task SimpleLoadTest()
        {
            var result = await LoadTestRunner.ExecuteAsync(async () =>
            {
                // Simple test that should complete quickly
                await Task.Delay(10);
                return true;
            });

            // Basic assertions
            Assert.True(result.Total > 0, "Should have executed at least one request");
            Assert.True(result.RequestsPerSecond > 0, "Should have measurable throughput");
        }

        [Fact]
        public async Task FluentAPITest()
        {
            var result = await LoadTestRunner.Create()
                .WithConcurrency(1)
                .WithDuration(500)
                .WithInterval(100)
                .WithName("Simple Fluent Test")
                .RunAsync(async () =>
                {
                    await Task.Delay(5);
                });

            Assert.True(result.Total > 0, "Should have executed at least one request");
        }
    }
}
