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
        public static async Task<LoadResult> Run(LoadExecutionPlan executionPlan)
        {
            if (executionPlan.Action == null)
                throw new ArgumentNullException(nameof(executionPlan.Action));

            // Concurrency is handled by the worker, so we only spawn one worker actor here.
            using var actorSystem = ActorSystem.Create("LoadTestSystem");
            var resultCollector = actorSystem.ActorOf(
                Props.Create(() => new ResultCollectorActor(executionPlan.Name)),
                "resultCollector"
            );

            // Create a single worker actor (no for-loop for concurrency).
            var worker = actorSystem.ActorOf(
                Props.Create(() => new LoadWorkerActor(executionPlan, resultCollector)),
                "worker"
            );

            // Ask the worker to start and wait for its final LoadResult.
            await worker.Ask<LoadResult>(
                new StartLoadMessage(),
                TimeSpan.FromSeconds(executionPlan.Settings.Duration.TotalSeconds + 5)
            );

            // Ask the result collector for the aggregated results.
            var finalResult = await resultCollector.Ask<LoadResult>(
                new GetLoadResultMessage(),
                TimeSpan.FromSeconds(executionPlan.Settings.Duration.TotalSeconds + 5)
            );

            return finalResult;
        }
    }
}