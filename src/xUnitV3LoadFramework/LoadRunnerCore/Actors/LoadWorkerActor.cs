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

            // Track all running tasks
            var runningTasks = new List<Task>();
            
            // Calculate how many batches we expect to run
            var expectedBatches = (int)Math.Ceiling(_executionPlan.Settings.Duration.TotalMilliseconds / _executionPlan.Settings.Interval.TotalMilliseconds);
            var expectedTotalRequests = expectedBatches * _executionPlan.Settings.Concurrency;
            
            _logger.Info("LoadWorkerActor '{0}' starting. Expected batches: {1}, Expected total requests: {2}", 
                workerName, expectedBatches, expectedTotalRequests);

            try
            {
                // Use a timer to ensure consistent intervals
                var startTime = DateTime.UtcNow;
                var batchNumber = 0;

                while (!cts.Token.IsCancellationRequested)
                {
                    var currentTime = DateTime.UtcNow;
                    var elapsedTime = currentTime - startTime;
                    
                    // Calculate when this batch should have started
                    var expectedBatchStartTime = TimeSpan.FromMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                    
                    // If we've exceeded the duration, stop
                    if (elapsedTime >= _executionPlan.Settings.Duration)
                    {
                        break;
                    }

                    // Start the batch of tasks
                    var batchTasks = new List<Task>();
                    for (int i = 0; i < _executionPlan.Settings.Concurrency; i++)
                    {
                        var task = ExecuteActionAsync(workerName, cts.Token);
                        batchTasks.Add(task);
                        runningTasks.Add(task);
                    }

                    _logger.Debug("[{0}] Batch {1} started at {2:F2}ms (expected: {3:F2}ms). Tasks in batch: {4}", 
                        workerName, batchNumber + 1, elapsedTime.TotalMilliseconds, 
                        expectedBatchStartTime.TotalMilliseconds, batchTasks.Count);

                    // Remove completed tasks
                    runningTasks.RemoveAll(t => t.IsCompleted);

                    batchNumber++;

                    // Calculate when the next batch should start
                    var nextBatchStartTime = TimeSpan.FromMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                    var timeUntilNextBatch = startTime.Add(nextBatchStartTime) - DateTime.UtcNow;

                    // If we're behind schedule, log it but continue immediately
                    if (timeUntilNextBatch <= TimeSpan.Zero)
                    {
                        _logger.Warning("[{0}] Running behind schedule. Next batch should have started {1:F2}ms ago", 
                            workerName, -timeUntilNextBatch.TotalMilliseconds);
                    }
                    else if (!cts.Token.IsCancellationRequested && elapsedTime.Add(timeUntilNextBatch) < _executionPlan.Settings.Duration)
                    {
                        // Wait until the next batch should start
                        try
                        {
                            await Task.Delay(timeUntilNextBatch, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            // Expected when duration expires
                            break;
                        }
                    }
                    else
                    {
                        // We've reached the end of the test duration
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors that occur during the load test
                _logger.Error(ex, "LoadWorkerActor '{0}' encountered an unexpected error.", workerName);
            }
            finally
            {
                // Log status before waiting for tasks
                _logger.Info("LoadWorkerActor '{0}' finished spawning tasks. Waiting for {1} tasks to complete.", 
                    workerName, runningTasks.Count(t => !t.IsCompleted));

                // Wait for all remaining tasks with a reasonable timeout
                try
                {
                    var completionTimeout = TimeSpan.FromSeconds(Math.Max(30, _executionPlan.Settings.Duration.TotalSeconds));
                    var allTasks = Task.WhenAll(runningTasks.Where(t => !t.IsCompleted));
                    
                    if (await Task.WhenAny(allTasks, Task.Delay(completionTimeout)) != allTasks)
                    {
                        _logger.Warning("LoadWorkerActor '{0}' timed out waiting for tasks to complete.", workerName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning("LoadWorkerActor '{0}' error while waiting for tasks: {1}", workerName, ex.Message);
                }

                // Log the completion of the load test
                _logger.Info("LoadWorkerActor '{0}' has completed load testing.", workerName);

                // Request the final load test results from the result collector
                var finalResult = await _resultCollector.Ask<LoadResult>(
                    new GetLoadResultMessage(), TimeSpan.FromSeconds(5));

                // Send the final result back to the original sender
                Sender.Tell(finalResult);
            }
        }

        private async Task ExecuteActionAsync(string workerName, CancellationToken cancellationToken)
        {
            try
            {
                // Notify that we're starting a request
                _resultCollector.Tell(new RequestStartedMessage());
                
                // Measure the execution time of the action
                var stopwatch = Stopwatch.StartNew();
                bool result = await _executionPlan.Action();
                stopwatch.Stop();
                var latency = stopwatch.Elapsed.TotalMilliseconds;

                // Send the result and latency to the result collector
                _resultCollector.Tell(new StepResultMessage(result, latency));

                // Log the result and latency for debugging
                _logger.Debug("[{0}] Task completed - Result: {1}, Latency: {2:F2} ms", 
                    workerName, result, latency);
            }
            catch (Exception ex)
            {
                // Report failure
                _resultCollector.Tell(new StepResultMessage(false, 0));
                _logger.Error(ex, "[{0}] Task failed with error", workerName);
            }
        }
    }
}