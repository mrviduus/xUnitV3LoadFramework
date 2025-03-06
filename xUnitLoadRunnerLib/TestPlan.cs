using System;
using System.Threading;
using System.Threading.Tasks;

namespace xUnitLoadRunnerLib
{
    public class TestPlan
    {
        public string Name { get; set; }
        public TestStep[] Steps { get; set; }
        public int Concurrency { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }

        public async Task Run()
        {
            var cts = new CancellationTokenSource(Duration);
            var tasks = new Task[Concurrency];
            int total = 0, success = 0, failure = 0;

            for (int i = 0; i < Concurrency; i++)
            {
                tasks[i] = RunStepsAsync(cts.Token,  total, success, failure);
            }

            await Task.WhenAll(tasks);
        }

        private async Task RunStepsAsync(CancellationToken token, int total, int success, int failure)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var step in Steps)
                {
                    Interlocked.Increment(ref total);
                    bool result = await step.Action();
                    if (result) Interlocked.Increment(ref success);
                    else Interlocked.Increment(ref failure);

                    await Task.Delay(Interval, token);
                }
            }
        }
    }
}