using System;
using System.Threading.Tasks;
using Akka.Actor;
using LoadTestRunner.Actors;
using LoadTestRunner.Messages;
using LoadTestRunner.Models;

namespace LoadTestRunner
{
    public static class LoadTestRunner
    {
        public static async Task<LoadTestResult> Run(LoadTestPlan plan)
        {
            if (plan.Steps == null)
                throw new ArgumentNullException(nameof(plan.Steps));
            if (plan.Steps.Length == 0 || plan.Settings.Concurrency == 0)
                return new LoadTestResult { ScenarioName = plan.Name };

            using (var actorSystem = ActorSystem.Create("LoadTestSystem"))
            {
                var resultCollector = actorSystem.ActorOf(Props.Create(() => new ResultCollectorActor(plan.Name)), "resultCollector");

                var tasks = new Task[plan.Settings.Concurrency];
                for (int i = 0; i < plan.Settings.Concurrency; i++)
                {
                    var worker = actorSystem.ActorOf(Props.Create(() => new LoadTestWorkerActor(plan, resultCollector)), $"worker-{i}");
                    tasks[i] = worker.Ask<LoadTestResult>(
                        new StartLoadTestMessage(),
                        TimeSpan.FromSeconds(plan.Settings.Duration.TotalSeconds + 5)
                    );
                }

                await Task.WhenAll(tasks);

                var result = await resultCollector.Ask<LoadTestResult>(
                    new GetLoadTestResultMessage(),
                    TimeSpan.FromSeconds(plan.Settings.Duration.TotalSeconds + 5)
                );
                return result;
            }
        }
    }
}