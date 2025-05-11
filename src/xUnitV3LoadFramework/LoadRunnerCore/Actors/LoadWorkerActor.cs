using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    public class LoadWorkerActor : ReceiveActor
    {
        // The execution plan that defines the load test configuration and action
        private readonly LoadExecutionPlan _executionPlan;

        // Reference to the actor responsible for collecting test results
        private readonly IActorRef _resultCollector;

        // Logger for logging messages and errors
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        // Constructor to initialize the actor with the execution plan and result collector
        public LoadWorkerActor(LoadExecutionPlan executionPlan, IActorRef resultCollector)
        {
            _executionPlan = executionPlan;
            _resultCollector = resultCollector;

            // Define the behavior for receiving a StartLoadMessage
            ReceiveAsync<StartLoadMessage>(async _ => await RunWorkAsync());
        }

        // Core method to execute the load test
        private async Task RunWorkAsync()
        {
            // Get the name of the current actor for logging purposes
            var workerName = Self.Path.Name;
            
            // Notify the result collector that the load test has started
            _resultCollector.Tell(new StartLoadMessage());
            
            // Create a cancellation token that will expire after the test duration
            using var cts = new CancellationTokenSource(_executionPlan.Settings.Duration);

            try
            {
                // Log the start of the load test
                _logger.Info("LoadWorkerActor '{0}' started load test.", workerName);

                // Loop until the cancellation token is triggered
                while (!cts.Token.IsCancellationRequested)
                {
                    // Create a collection of tasks to execute the load test concurrently
                    var tasks = Enumerable.Range(0, _executionPlan.Settings.Concurrency)
                        .Select(_ => Task.Run(async () =>
                        {
                            // Measure the execution time of the action
                            var stopwatch = Stopwatch.StartNew();
                            bool result = await _executionPlan.Action();
                            stopwatch.Stop();
                            var latency = stopwatch.Elapsed.TotalMilliseconds;

                            // Send the result and latency to the result collector
                            _resultCollector.Tell(new StepResultMessage(result, latency));

                            // Log the result and latency for debugging
                            _logger.Debug("[{0}] Result: {1}, Latency: {2:F2} ms", workerName, result, latency);
                        }, cts.Token))
                        .ToArray();

                    // Wait for all tasks to complete
                    await Task.WhenAll(tasks);

                    // If the cancellation token is not triggered, wait for the specified interval
                    if (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(_executionPlan.Settings.Interval, cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Log a warning if the load test was canceled due to duration expiration
                _logger.Warning("LoadWorkerActor '{0}' load test canceled due to duration expiration.", workerName);
            }
            catch (Exception ex)
            {
                // Log any unexpected errors that occur during the load test
                _logger.Error(ex, "LoadWorkerActor '{0}' encountered an unexpected error.", workerName);
            }
            finally
            {
                // Log the completion of the load test
                _logger.Info("LoadWorkerActor '{0}' has completed load testing.", workerName);

                // Request the final load test results from the result collector
                var finalResult = await _resultCollector.Ask<LoadResult>(
                    new GetLoadResultMessage(), TimeSpan.FromSeconds(5));

                // Send the final result back to the original sender
                Sender.Tell(finalResult);
            }
        }
    }
}