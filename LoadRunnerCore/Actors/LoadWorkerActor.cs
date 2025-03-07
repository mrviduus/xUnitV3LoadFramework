using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using LoadRunnerCore.Messages;
using LoadRunnerCore.Models;

namespace LoadRunnerCore.Actors
{
    public class LoadWorkerActor : ReceiveActor
    {
        private readonly LoadPlan _plan;
        private readonly IActorRef _resultCollector;
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        public LoadWorkerActor(LoadPlan plan, IActorRef resultCollector)
        {
            _plan = plan;
            _resultCollector = resultCollector;
            ReceiveAsync<StartLoadMessage>(async _ => await RunStepsAsync());
        }

        private async Task RunStepsAsync()
        {
            // Capture what you need from the actor context here
            var workerName = Self.Path.Name;

            using var cts = new CancellationTokenSource(_plan.Settings.Duration);
            try
            {
                _logger.Info("Worker {0} started load test steps (parallel execution).", workerName);

                // Repeat until duration expires or token is canceled
                while (!cts.Token.IsCancellationRequested)
                {
                    // Run all steps in parallel for this iteration
                    var stepTasks = _plan.Steps.Select(step =>
                    {
                        return Task.Run(async () =>
                        {
                            var stepResult = await step.Action();
                            _resultCollector.Tell(new StepResultMessage(stepResult));
                            _logger.Debug("Worker {0} step result: {1}", workerName, stepResult);
                        }, cts.Token);
                    }).ToArray();

                    // Wait for all parallel tasks to complete for this "batch" of steps
                    await Task.WhenAll(stepTasks);

                    // If cancellation has not occurred, wait for the configured interval before the next batch
                    if (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(_plan.Settings.Interval, cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger.Info("Worker {0} canceled during parallel step execution.", workerName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Worker {0} encountered an error.", workerName);
            }
            finally
            {
                _logger.Info("Worker {0} returning final result.", workerName);
                Sender.Tell(new LoadResult());
            }
        }
    }
}