using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace xUnitLoadRunnerLib
{
    public static class LoadTestRunner
    {
        public static async Task<LoadTestResult> Run(LoadTestPlan plan)
        {
            if (plan.Steps == null)
                throw new ArgumentNullException(nameof(plan.Steps));
            if (plan.Steps.Length == 0 || plan.Settings.Concurrency == 0)
                return new LoadTestResult(){ScenarioName = plan.Name};

            var cts = new CancellationTokenSource(plan.Settings.Duration);
            var tasks = new Task[plan.Settings.Concurrency];

            var total = new ConcurrentBag<int>();
            var success = new ConcurrentBag<int>();
            var failure = new ConcurrentBag<int>();

            for (int i = 0; i < plan.Settings.Concurrency; i++)
            {
                tasks[i] = RunStepsAsync(plan, cts.Token, total, success, failure);
            }

            await Task.WhenAll(tasks);

            return new LoadTestResult
            {
                Total = total.Count,
                Success = success.Count,
                Failure = failure.Count
            };
        }

        private static async Task RunStepsAsync(LoadTestPlan plan,
            CancellationToken token,
            ConcurrentBag<int> total,
            ConcurrentBag<int> success,
            ConcurrentBag<int> failure)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var step in plan.Steps)
                    {
                        total.Add(1);
                        var result = await step.Action();
                        if (result)
                            success.Add(1);
                        else
                            failure.Add(1);

                        await Task.Delay(plan.Settings.Interval, token);
                        if (token.IsCancellationRequested)
                            break;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }

        }
    }
}