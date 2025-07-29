// Import System.Diagnostics for high-precision timing measurements using Stopwatch
// Essential for accurate latency tracking and performance monitoring
using System.Diagnostics;
// Import Akka.NET actor framework for message-driven concurrency and fault tolerance
// Provides the foundation for scalable, distributed load testing architecture
using Akka.Actor;
// Import Akka.Event for structured logging within the actor system
// Enables centralized logging with proper context and severity levels
using Akka.Event;
// Import message contracts for inter-actor communication during load testing
// Defines all message types used for coordination and result reporting
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
// Import data models for load test configuration and result structures
// Contains execution plans, settings, and result aggregation models
using xUnitV3LoadFramework.LoadRunnerCore.Models;

// Define namespace for load testing actor implementations
namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    /// <summary>
    /// Actor responsible for executing load tests using Task-based concurrent execution.
    /// Manages the lifecycle of load test execution with precise timing control and batch processing.
    /// Implements the Task-based execution mode for moderate concurrency scenarios.
    /// </summary>
    public class LoadWorkerActor : ReceiveActor
    {
        // Store the execution plan that defines test configuration, duration, and action to execute
        // This immutable configuration drives all aspects of the load test execution
        private readonly LoadExecutionPlan _executionPlan;

        // Reference to the result collector actor for sending performance metrics and test results
        // All timing data, success/failure counts, and latency measurements are sent here
        private readonly IActorRef _resultCollector;

        // Akka.NET logging adapter for structured logging with actor context information
        // Provides consistent logging format with actor path, timestamps, and severity levels
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        /// <summary>
        /// Constructor to initialize the LoadWorkerActor with execution plan and result collector.
        /// Sets up message handling patterns and establishes communication channels.
        /// </summary>
        /// <param name="executionPlan">The configuration defining test duration, concurrency, and action</param>
        /// <param name="resultCollector">Actor reference for sending test results and metrics</param>
        public LoadWorkerActor(LoadExecutionPlan executionPlan, IActorRef resultCollector)
        {
            // Store the execution plan for use throughout the actor's lifecycle
            // This contains all test configuration including timing, concurrency, and test action
            _executionPlan = executionPlan;
            
            // Store reference to result collector for sending performance data
            // All test results, latency measurements, and status updates go through this actor
            _resultCollector = resultCollector;

            // Define asynchronous message handler for StartLoadMessage
            // When received, this triggers the main load test execution workflow
            ReceiveAsync<StartLoadMessage>(async _ => await RunWorkAsync());
        }

        /// <summary>
        /// Core method that orchestrates the entire load test execution workflow.
        /// Manages timing, batch processing, task coordination, and result collection.
        /// Implements precise interval control and comprehensive error handling.
        /// </summary>
        private async Task RunWorkAsync()
        {
            // Extract actor name from the actor path for consistent logging and identification
            // This provides a unique identifier for tracking this specific worker instance
            var workerName = Self.Path.Name;
            
            // Notify the result collector that load test execution has begun
            // This starts the timing clock and initializes result collection state
            _resultCollector.Tell(new StartLoadMessage());
            
            // Create cancellation token that will automatically expire after the configured test duration
            // This ensures the test stops exactly when specified, preventing runaway execution
            using var cts = new CancellationTokenSource(_executionPlan.Settings.Duration);

            // Initialize collection to track all spawned tasks for proper cleanup and monitoring
            // This allows us to wait for completion and track outstanding work
            var runningTasks = new List<Task>();
            
            // Calculate the expected number of batches based on test duration and interval
            // Used for capacity planning and progress monitoring throughout the test
            var expectedBatches = (int)Math.Ceiling(_executionPlan.Settings.Duration.TotalMilliseconds / _executionPlan.Settings.Interval.TotalMilliseconds);
            
            // Calculate total expected requests for resource planning and validation
            // Helps predict memory usage and validate final results against expectations
            var expectedTotalRequests = expectedBatches * _executionPlan.Settings.Concurrency;
            
            // Log test initialization with key parameters for monitoring and debugging
            // Provides visibility into test scope and expected resource requirements
            _logger.Info("LoadWorkerActor '{0}' starting. Expected batches: {1}, Expected total requests: {2}", 
                workerName, expectedBatches, expectedTotalRequests);

            try
            {
                // Capture the precise start time for accurate interval calculation
                // This timestamp is used as the reference point for all batch timing
                var startTime = DateTime.UtcNow;
                
                // Initialize batch counter for tracking progress and logging purposes
                // Each batch represents one interval cycle with the configured concurrency
                var batchNumber = 0;

                // Main execution loop - continue until cancellation token is triggered or duration expires
                // This loop implements the core load generation pattern with precise timing
                while (!cts.Token.IsCancellationRequested)
                {
                    // Capture current time for elapsed time calculation and timing accuracy
                    // Used to determine if we're on schedule and calculate precise delays
                    var currentTime = DateTime.UtcNow;
                    var elapsedTime = currentTime - startTime;
                    
                    // Calculate the theoretical time when this batch should have started
                    // This is used for timing accuracy and detecting schedule drift
                    var expectedBatchStartTime = TimeSpan.FromMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                    
                    // Check if we've exceeded the configured test duration
                    // This provides a secondary safety check beyond the cancellation token
                    if (elapsedTime >= _executionPlan.Settings.Duration)
                    {
                        // Exit the loop cleanly when test duration is reached
                        break;
                    }

                    // Initialize collection for tracking tasks within this specific batch
                    // Allows for batch-level monitoring and debugging capabilities
                    var batchTasks = new List<Task>();
                    
                    // Create the specified number of concurrent tasks for this batch
                    // Each task represents one execution of the test action
                    for (int i = 0; i < _executionPlan.Settings.Concurrency; i++)
                    {
                        // Create and start a new task for executing the test action
                        // Each task runs independently but is tracked for completion
                        var task = ExecuteActionAsync(workerName, cts.Token);
                        batchTasks.Add(task);
                        runningTasks.Add(task);
                    }

                    // Log detailed batch information for debugging and monitoring
                    // Includes timing accuracy metrics to detect and diagnose scheduling issues
                    _logger.Debug("[{0}] Batch {1} started at {2:F2}ms (expected: {3:F2}ms). Tasks in batch: {4}", 
                        workerName, batchNumber + 1, elapsedTime.TotalMilliseconds, 
                        expectedBatchStartTime.TotalMilliseconds, batchTasks.Count);

                    // Clean up completed tasks to prevent memory accumulation over long tests
                    // This prevents the task list from growing unboundedly during extended execution
                    runningTasks.RemoveAll(t => t.IsCompleted);

                    // Increment batch counter for next iteration and timing calculations
                    // This drives the interval calculation for the subsequent batch
                    batchNumber++;

                    // Calculate the precise time when the next batch should start
                    // This maintains accurate timing regardless of execution delays
                    var nextBatchStartTime = TimeSpan.FromMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                    var timeUntilNextBatch = startTime.Add(nextBatchStartTime) - DateTime.UtcNow;

                    // Detect and handle schedule drift - when execution falls behind the intended timing
                    // This is critical for maintaining consistent load patterns
                    if (timeUntilNextBatch <= TimeSpan.Zero)
                    {
                        // Log warning about timing drift but continue immediately
                        // This helps identify performance bottlenecks or resource constraints
                        _logger.Warning("[{0}] Running behind schedule. Next batch should have started {1:F2}ms ago", 
                            workerName, -timeUntilNextBatch.TotalMilliseconds);
                    }
                    else if (!cts.Token.IsCancellationRequested && elapsedTime.Add(timeUntilNextBatch) < _executionPlan.Settings.Duration)
                    {
                        // Wait until the precise time for the next batch to start
                        // This implements accurate interval timing for consistent load generation
                        try
                        {
                            // Use cancellation token to allow early termination during delay
                            await Task.Delay(timeUntilNextBatch, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            // Expected exception when test duration expires during delay
                            // This is the normal path for test completion
                            break;
                        }
                    }
                    else
                    {
                        // Exit condition: we've reached the end of the configured test duration
                        // This ensures we don't start new batches beyond the intended test time
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log any unexpected errors that occur during load test execution
                // This provides debugging information while allowing graceful degradation
                _logger.Error(ex, "LoadWorkerActor '{0}' encountered an unexpected error.", workerName);
            }
            finally
            {
                // Log the current state before beginning cleanup operations
                // Provides visibility into how many tasks are still pending completion
                _logger.Info("LoadWorkerActor '{0}' finished spawning tasks. Waiting for {1} tasks to complete.", 
                    workerName, runningTasks.Count(t => !t.IsCompleted));

                // Wait for all outstanding tasks to complete with a reasonable timeout
                // This ensures we capture all results before finalizing the test
                try
                {
                    // Calculate timeout that allows enough time for tasks to complete
                    // Uses the longer of 30 seconds or the test duration to accommodate various scenarios
                    var completionTimeout = TimeSpan.FromSeconds(Math.Max(30, _executionPlan.Settings.Duration.TotalSeconds));
                    
                    // Create a composite task that completes when all running tasks finish
                    // This allows us to wait for everything with a single timeout
                    var allTasks = Task.WhenAll(runningTasks.Where(t => !t.IsCompleted));
                    
                    // Wait for either all tasks to complete or timeout to expire
                    // This prevents indefinite hanging while allowing reasonable completion time
                    if (await Task.WhenAny(allTasks, Task.Delay(completionTimeout)) != allTasks)
                    {
                        // Log warning if some tasks didn't complete within the timeout
                        // This indicates potential issues with the test action or system performance
                        _logger.Warning("LoadWorkerActor '{0}' timed out waiting for tasks to complete.", workerName);
                    }
                }
                catch (Exception ex)
                {
                    // Log any errors during task completion wait without failing the entire test
                    // This ensures we still get partial results even if cleanup encounters issues
                    _logger.Warning("LoadWorkerActor '{0}' error while waiting for tasks: {1}", workerName, ex.Message);
                }

                // Log successful completion of the load test execution phase
                // Indicates that the worker has finished its primary responsibility
                _logger.Info("LoadWorkerActor '{0}' has completed load testing.", workerName);

                // Request the final aggregated results from the result collector actor
                // This triggers result calculation and returns comprehensive performance metrics
                var finalResult = await _resultCollector.Ask<LoadResult>(
                    new GetLoadResultMessage(), TimeSpan.FromSeconds(5));

                // Send the final consolidated results back to the test runner
                // This completes the actor communication chain and provides results to the caller
                Sender.Tell(finalResult);
            }
        }

        /// <summary>
        /// Executes a single test action with precise timing measurement and error handling.
        /// Reports results to the result collector and handles both success and failure scenarios.
        /// Implements comprehensive latency tracking and exception management.
        /// </summary>
        /// <param name="workerName">Identifier for this worker instance for logging purposes</param>
        /// <param name="cancellationToken">Token to allow early termination of the action</param>
        private async Task ExecuteActionAsync(string workerName, CancellationToken cancellationToken)
        {
            try
            {
                // Notify result collector that a new request is starting
                // This is used for tracking in-flight requests and calculating accurate throughput
                _resultCollector.Tell(new RequestStartedMessage());
                
                // Create high-precision stopwatch for accurate latency measurement
                // Stopwatch provides the most accurate timing available on the platform
                var stopwatch = Stopwatch.StartNew();
                
                // Execute the actual test action and capture the result
                // This is the user-defined action that represents the workload being tested
                bool result = await _executionPlan.Action();
                
                // Stop timing immediately after action completion for accuracy
                // Minimizes measurement overhead in the latency calculation
                stopwatch.Stop();
                
                // Convert elapsed time to milliseconds for standard latency reporting
                // Milliseconds provide appropriate precision for most performance scenarios
                var latency = stopwatch.Elapsed.TotalMilliseconds;

                // Send the test result and measured latency to the result collector
                // This data is aggregated for final performance metrics and percentile calculations
                _resultCollector.Tell(new StepResultMessage(result, latency));

                // Log detailed execution information for debugging and monitoring
                // Includes both the success/failure status and precise latency measurement
                _logger.Debug("[{0}] Task completed - Result: {1}, Latency: {2:F2} ms", 
                    workerName, result, latency);
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during test action execution
                // Report as failure with zero latency since timing is not meaningful
                _resultCollector.Tell(new StepResultMessage(false, 0));
                
                // Log the error with full exception details for debugging
                // This helps identify issues with the test action or environment
                _logger.Error(ex, "[{0}] Task failed with error", workerName);
            }
        }
    }
}