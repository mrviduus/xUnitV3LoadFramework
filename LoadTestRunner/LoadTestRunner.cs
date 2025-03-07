using System;
using System.Threading.Tasks;
using Akka.Actor;
using LoadTestRunner.Actors;
using LoadTestRunner.Messages;
using LoadTestRunner.Models;

namespace LoadTestRunner
{
    public class StartLoadTestMessage { }
    public static class LoadTestRunner
    {
        private static ActorSystem _actorSystem;
        private static IActorRef _resultCollector;

        static LoadTestRunner()
        {
            _actorSystem = ActorSystem.Create("LoadTestSystem");
        }

        public static async Task<LoadTestResult> Run(LoadTestPlan plan)
        {
            if (plan.Steps == null)
                throw new ArgumentNullException(nameof(plan.Steps));
            if (plan.Steps.Length == 0 || plan.Settings.Concurrency == 0)
                return new LoadTestResult() { ScenarioName = plan.Name };

            _resultCollector = _actorSystem.ActorOf(Props.Create(() => new ResultCollectorActor(plan.Name)), "resultCollector");

            var tasks = new Task[plan.Settings.Concurrency];
            for (int i = 0; i < plan.Settings.Concurrency; i++)
            {
                var worker = _actorSystem.ActorOf(Props.Create(() => new LoadTestWorkerActor(plan, _resultCollector)), $"worker-{i}");
                tasks[i] = worker.Ask<LoadTestResult>(new StartLoadTestMessage(), TimeSpan.FromMinutes(1));
            }

            await Task.WhenAll(tasks);
            var result = await _resultCollector.Ask<LoadTestResult>(new GetLoadTestResultMessage(), TimeSpan.FromMinutes(1));

            return result;
        }
    }
}