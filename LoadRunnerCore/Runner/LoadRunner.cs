using System;
using System.Threading.Tasks;
using Akka.Actor;
using LoadRunnerCore.Actors;
using LoadRunnerCore.Messages;
using LoadRunnerCore.Models;

namespace LoadRunnerCore.Runner
{
    public static class LoadRunner
    {
        public static async Task<LoadResult> Run(LoadPlan plan)
        {
            if (plan.Steps == null)
                throw new ArgumentNullException(nameof(plan.Steps));
            if (plan.Steps.Length == 0 || plan.Settings.Concurrency == 0)
                return new LoadResult { ScenarioName = plan.Name };

            using var actorSystem = ActorSystem.Create("LoadTestSystem");
            var resultCollector = actorSystem.ActorOf(Props.Create(() => new ResultCollectorActor(plan.Name)), "resultCollector");

            var tasks = new Task[plan.Settings.Concurrency];
            for (int i = 0; i < plan.Settings.Concurrency; i++)
            {
                var worker = actorSystem.ActorOf(Props.Create(() => new LoadWorkerActor(plan, resultCollector)), $"worker-{i}");
                tasks[i] = worker.Ask<LoadResult>(
                    new StartLoadMessage(),
                    TimeSpan.FromSeconds(plan.Settings.Duration.TotalSeconds + 5)
                );
            }

            await Task.WhenAll(tasks);

            var result = await resultCollector.Ask<LoadResult>(
                new GetLoadResultMessage(),
                TimeSpan.FromSeconds(plan.Settings.Duration.TotalSeconds + 5)
            );
            return result;
        }
    }
}