using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace xUnitLoadRunnerLib
{

    public class LoadTestWorkerActor : ReceiveActor
    {
        private readonly LoadTestPlan _plan;
        private readonly IActorRef _resultCollector;

        public LoadTestWorkerActor(LoadTestPlan plan, IActorRef resultCollector)
        {
            _plan = plan;
            _resultCollector = resultCollector;

            ReceiveAsync<StartLoadTestMessage>(async _ => await RunStepsAsync());
        }

        private async Task RunStepsAsync()
        {
            var cts = new CancellationTokenSource(_plan.Settings.Duration);
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    foreach (var step in _plan.Steps)
                    {
                        var result = await step.Action();
                        _resultCollector.Tell(new StepResultMessage(result));

                        await Task.Delay(_plan.Settings.Interval, cts.Token);
                        if (cts.Token.IsCancellationRequested)
                            break;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation gracefully
            }
            finally
            {
                Sender.Tell(new LoadTestResult());
            }
        }
    }
}