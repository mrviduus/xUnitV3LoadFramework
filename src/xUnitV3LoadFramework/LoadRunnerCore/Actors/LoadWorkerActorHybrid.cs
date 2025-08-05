using System.Diagnostics;
using System.Threading.Channels;
using Akka.Actor;
using Akka.Event;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    /// <summary>
    /// Hybrid implementation of LoadWorkerActor optimized for high-throughput scenarios (100k+ requests).
    /// Uses a fixed pool of worker tasks with channels for efficient work distribution.
    /// Prevents thread pool exhaustion and provides superior scalability compared to Task-based approach.
    /// Implements producer-consumer pattern with unbounded channels for maximum performance.
    /// </summary>
    public class LoadWorkerActorHybrid : ReceiveActor
    {
        // Store the execution plan that defines test configuration, duration, and action to execute
        // This immutable configuration drives all aspects of the hybrid load test execution
        private readonly LoadExecutionPlan _executionPlan;

        // Reference to the result collector actor for sending performance metrics and test results
        // All timing data, success/failure counts, latency, and queue time measurements are sent here
        private readonly IActorRef _resultCollector;

        // Akka.NET logging adapter for structured logging with actor context information
        // Provides consistent logging format with actor path, timestamps, and severity levels
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        // High-performance unbounded channel for distributing work items to worker threads
        // Implements producer-consumer pattern for optimal throughput with minimal blocking
        private readonly Channel<WorkItem> _workChannel;

        // Collection of dedicated worker tasks that process work items from the channel
        // Fixed pool size prevents resource exhaustion while maximizing concurrent processing
        private readonly List<Task> _workerTasks;

        // Cancellation token source for graceful shutdown of all worker threads
        // Allows coordinated termination of the entire worker pool when test completes
        private readonly CancellationTokenSource _workerCts = new();

        // Optimal number of worker threads calculated based on system resources and concurrency requirements
        // Balances resource utilization with performance to prevent thread starvation or excess overhead
        private readonly int _workerCount;

        /// <summary>
        /// Constructor to initialize the LoadWorkerActorHybrid with execution plan and result collector.
        /// Sets up the channel-based work distribution system and calculates optimal worker count.
        /// Creates the worker task pool and establishes message handling patterns.
        /// </summary>
        /// <param name="executionPlan">The configuration defining test duration, concurrency, and action</param>
        /// <param name="resultCollector">Actor reference for sending test results and performance metrics</param>
        public LoadWorkerActorHybrid(LoadExecutionPlan executionPlan, IActorRef resultCollector)
        {
            // Store the execution plan for use throughout the actor's lifecycle
            // This contains all test configuration including timing, concurrency, and test action
            _executionPlan = executionPlan;
            
            // Store reference to result collector for sending performance data and metrics
            // All test results, latency measurements, queue times, and status updates go through this actor
            _resultCollector = resultCollector;

            // Create high-performance unbounded channel for work item distribution
            // Unbounded design prevents blocking while maintaining thread safety for multiple producers/consumers
            _workChannel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
            {
                // Allow multiple workers to read concurrently for maximum throughput
                SingleReader = false,
                // Allow scheduler to write while workers are reading for continuous flow
                SingleWriter = false
            });

            // Calculate the optimal number of worker threads based on system capabilities and load requirements
            // This balances resource utilization with performance to prevent bottlenecks or waste
            _workerCount = CalculateOptimalWorkerCount(executionPlan.Settings.Concurrency);
            
            // Pre-allocate worker task collection with exact capacity for memory efficiency
            // Fixed size prevents dynamic resizing overhead during test execution
            _workerTasks = new List<Task>(_workerCount);

            // Define asynchronous message handler for StartLoadMessage to begin hybrid execution
            // When received, this triggers the main hybrid load test execution workflow
            ReceiveAsync<StartLoadMessage>(async message =>
            {
                try
                {
                    await RunWorkAsync();
                }
                catch (Exception ex)
                {
                    // Log error and send failure result
                    _logger.Error(ex, "LoadWorkerActorHybrid failed during execution");
                    Sender.Tell(new LoadResult 
                    { 
                        ScenarioName = _executionPlan.Name,
                        Success = 0, 
                        Failure = 1, 
                        Total = 1, 
                        Time = 0,
                        RequestsPerSecond = 0,
                        AverageLatency = 0
                    });
                }
            });
        }

        /// <summary>
        /// Calculates the optimal number of worker threads based on system resources and concurrency requirements.
        /// Uses heuristics to balance CPU utilization, memory consumption, and expected workload characteristics.
        /// Considers both CPU core count and target concurrency to prevent resource starvation or excess overhead.
        /// </summary>
        /// <param name="concurrency">The target concurrency level from the execution plan</param>
        /// <returns>Optimal number of worker threads for maximum performance without resource exhaustion</returns>
        private int CalculateOptimalWorkerCount(int concurrency)
        {
            // Get the number of logical CPU cores available to the process
            // This forms the baseline for worker count calculations
            var coreCount = Environment.ProcessorCount;
            
            // Calculate base worker count assuming I/O bound workloads (2 workers per core)
            // I/O bound tasks can benefit from more workers than CPU cores due to blocking operations
            var baseWorkers = coreCount * 2;
            
            // Scale workers based on target concurrency with reasonable ratio (1 worker per 10 concurrent operations)
            // This prevents creating excessive workers while ensuring adequate parallel processing capacity
            var scaledWorkers = Math.Max(baseWorkers, concurrency / 10);
            
            // Set maximum worker limit to prevent resource exhaustion (50x cores or absolute cap of 1000)
            // This prevents scenarios where very high concurrency creates unmanageable worker counts
            var maxWorkers = Math.Min(1000, coreCount * 50);
            
            // Apply the maximum limit to the calculated worker count
            // Final result balances performance with resource constraints
            var optimalWorkers = Math.Min(scaledWorkers, maxWorkers);
            
            // Log the calculation details for monitoring and performance tuning
            // This helps understand resource allocation decisions and identify potential adjustments
            _logger.Info("Calculated optimal worker count: {0} (cores: {1}, concurrency: {2})", 
                optimalWorkers, coreCount, concurrency);
            
            // Return the calculated optimal worker count for pool creation
            return optimalWorkers;
        }

        /// <summary>
        /// Core method that orchestrates the entire hybrid load test execution workflow.
        /// Manages worker pool creation, work scheduling, timing control, and result collection.
        /// Implements high-performance producer-consumer pattern with channels for optimal throughput.
        /// </summary>
        private async Task RunWorkAsync()
        {
            // Extract actor name from the actor path for consistent logging and identification
            // This provides a unique identifier for tracking this specific hybrid worker instance
            var workerName = Self.Path.Name;
            
            // Notify the result collector that hybrid load test execution has begun
            // This starts the timing clock and initializes result collection state
            _resultCollector.Tell(new StartLoadMessage());
            
            // Inform result collector about the worker thread count for resource utilization tracking
            // This data is used for calculating worker efficiency and resource utilization metrics
            _resultCollector.Tell(new WorkerThreadCountMessage { ThreadCount = _workerCount });
            
            // Create cancellation token that will automatically expire after the configured test duration
            // This ensures the test stops exactly when specified, preventing runaway execution
            using var cts = new CancellationTokenSource(_executionPlan.Settings.Duration);
            
            // Create linked cancellation token that responds to both duration timeout and manual cancellation
            // This allows graceful shutdown from either source while maintaining proper cleanup
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _workerCts.Token);

            try
            {
                // Initialize and start the fixed pool of worker tasks for processing work items
                // Each worker operates independently, reading from the shared channel
                for (int i = 0; i < _workerCount; i++)
                {
                    // Capture worker ID for logging and debugging purposes
                    // Each worker maintains its own identity for tracking and monitoring
                    var workerId = i;
                    
                    // Create and start a dedicated worker task that processes items from the channel
                    // Workers run continuously until the channel is completed or cancellation occurs
                    _workerTasks.Add(ProcessWorkItems(workerId, linkedCts.Token));
                }

                // Start the scheduler task that produces work items according to the test timing
                // This task generates work items at precise intervals and feeds them to the channel
                var schedulerTask = ScheduleWorkItems(linkedCts.Token);

                // Wait for the configured test duration to elapse
                // This maintains precise timing control regardless of work item processing speed
                await Task.Delay(_executionPlan.Settings.Duration, linkedCts.Token);

                // Signal completion to all workers by completing the channel writer
                // This ensures no new work items are added and workers can finish processing
                _workChannel.Writer.TryComplete();

                // Wait for all worker tasks to complete processing their remaining work items
                // This ensures all scheduled work is finished before finalizing results
                await Task.WhenAll(_workerTasks);
                
                // Wait for the scheduler task to complete its work item generation
                // This ensures all scheduling operations are finished
                await schedulerTask;
            }
            catch (OperationCanceledException)
            {
                // Handle expected cancellation due to test duration expiration or manual stop
                // This is the normal termination path and doesn't indicate an error
                _logger.Debug("LoadWorkerActorHybrid '{0}' operation cancelled", workerName);
            }
            catch (Exception ex)
            {
                // Handle and log any unexpected errors that occur during hybrid load test execution
                // This provides debugging information while allowing graceful degradation
                _logger.Error(ex, "LoadWorkerActorHybrid '{0}' encountered an error", workerName);
            }
            finally
            {
                // Ensure the channel writer is completed even if exceptions occurred
                // This prevents workers from waiting indefinitely for new work items
                _workChannel.Writer.TryComplete();

                // Request the final aggregated results from the result collector actor
                // This triggers result calculation and returns comprehensive performance metrics
                var finalResult = await _resultCollector.Ask<LoadResult>(
                    new GetLoadResultMessage(), TimeSpan.FromSeconds(5));
                
                // Log comprehensive completion summary with key performance indicators
                // Provides immediate visibility into test results and resource utilization
                _logger.Info("LoadWorkerActorHybrid '{0}' completed. Total: {1}, Success: {2}, Failed: {3}, In-flight: {4}", 
                    workerName, finalResult.Total, finalResult.Success, finalResult.Failure, finalResult.RequestsInFlight);
                
                // Send the final consolidated results back to the test runner
                // This completes the actor communication chain and provides results to the caller
                Sender.Tell(finalResult);
            }
        }

        /// <summary>
        /// Schedules work items according to the configured interval and concurrency settings.
        /// Implements precise timing control for consistent load generation patterns.
        /// Feeds work items into the channel for consumption by worker threads.
        /// </summary>
        /// <param name="cancellationToken">Token to allow early termination of scheduling</param>
        private async Task ScheduleWorkItems(CancellationToken cancellationToken)
        {
            // Capture the precise start time for accurate interval calculation
            // This timestamp is used as the reference point for all batch timing
            var startTime = DateTime.UtcNow;
            
            // Initialize batch counter for tracking progress and logging purposes
            // Each batch represents one interval cycle with the configured concurrency
            var batchNumber = 0;
            
            // Track total work items scheduled for monitoring and validation
            // Used to verify that the expected number of items were generated
            var totalScheduled = 0;

            // Main scheduling loop - continue until cancellation or duration expires
            // This loop implements the core work generation pattern with precise timing
            while (!cancellationToken.IsCancellationRequested)
            {
                // Calculate elapsed time since test start for duration checking
                // Used to determine if we've exceeded the configured test duration
                var elapsedTime = DateTime.UtcNow - startTime;
                
                // Check if we've exceeded the configured test duration
                // This provides a secondary safety check beyond the cancellation token
                if (elapsedTime >= _executionPlan.Settings.Duration)
                    break;

                // Generate and schedule the configured number of work items for this batch
                // Each work item represents one execution of the test action
                for (int i = 0; i < _executionPlan.Settings.Concurrency; i++)
                {
                    // Create a new work item with unique identifier and timing information
                    // This data is used for queue time calculations and batch tracking
                    var workItem = new WorkItem
                    {
                        // Generate unique identifier for tracking and debugging purposes
                        Id = Guid.NewGuid(),
                        // Associate with current batch for performance analysis
                        BatchNumber = batchNumber,
                        // Record scheduling time for queue time measurement
                        ScheduledTime = DateTime.UtcNow
                    };

                    // Write the work item to the channel for worker consumption
                    // This is a high-performance operation that rarely blocks with unbounded channels
                    await _workChannel.Writer.WriteAsync(workItem, cancellationToken);
                    
                    // Increment total scheduled counter for monitoring purposes
                    totalScheduled++;
                }

                // Log batch scheduling information for monitoring and debugging
                // Provides visibility into work generation rate and total progress
                _logger.Debug("Batch {0} scheduled with {1} items. Total scheduled: {2}", 
                    batchNumber, _executionPlan.Settings.Concurrency, totalScheduled);

                // Notify result collector that this batch has been scheduled
                // This provides batch-level metrics and timing information for analysis
                _resultCollector.Tell(new BatchCompletedMessage(batchNumber, _executionPlan.Settings.Concurrency, DateTime.UtcNow));

                // Increment batch counter for next iteration and timing calculations
                // This drives the interval calculation for the subsequent batch
                batchNumber++;

                // Calculate the precise time when the next batch should be scheduled
                // This maintains accurate timing regardless of scheduling overhead
                var nextBatchTime = startTime.AddMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                var delay = nextBatchTime - DateTime.UtcNow;

                // Wait until the precise time for the next batch if we're ahead of schedule
                // This implements accurate interval timing for consistent load generation
                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        // Use cancellation token to allow early termination during delay
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected exception when test duration expires during delay
                        // This is the normal path for test completion
                        break;
                    }
                }
            }

            // Log completion summary with total work items scheduled
            // Provides final validation that expected work was generated
            _logger.Info("Work scheduling completed. Total items scheduled: {0}", totalScheduled);
        }

        /// <summary>
        /// Worker thread that continuously processes work items from the channel.
        /// Implements the consumer side of the producer-consumer pattern for high throughput.
        /// Each worker operates independently for maximum parallel processing efficiency.
        /// </summary>
        /// <param name="workerId">Unique identifier for this worker thread for logging purposes</param>
        /// <param name="cancellationToken">Token to allow graceful shutdown of the worker</param>
        private async Task ProcessWorkItems(int workerId, CancellationToken cancellationToken)
        {
            // Track the number of work items processed by this worker for monitoring
            // Used for performance analysis and load balancing validation
            var processedCount = 0;

            try
            {
                // Continuously read and process work items from the channel until completion or cancellation
                // The async enumerable pattern provides efficient, non-blocking consumption
                await foreach (var workItem in _workChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    // Process the individual work item with comprehensive error handling and timing
                    // Each work item execution is independent and won't affect other items
                    await ProcessSingleWorkItem(workItem, workerId);
                    
                    // Increment counter for monitoring and performance tracking
                    processedCount++;
                    
                    // Yield control periodically to prevent any single worker from monopolizing resources
                    // This ensures fair scheduling and prevents blocking other system operations
                    if (processedCount % 100 == 0)
                    {
                        await Task.Yield();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle expected cancellation due to test completion or shutdown
                // Log the final processing count for this worker
                _logger.Debug("Worker {0} cancelled after processing {1} items", workerId, processedCount);
            }
            catch (Exception ex)
            {
                // Handle and log any unexpected errors that occur during work item processing
                // Continue logging the processing count to aid in debugging
                _logger.Error(ex, "Worker {0} encountered error after processing {1} items", workerId, processedCount);
            }

            // Log worker completion with final processing statistics
            // Provides insight into work distribution and individual worker performance
            _logger.Debug("Worker {0} completed. Processed {1} items", workerId, processedCount);
        }

        /// <summary>
        /// Processes a single work item with comprehensive timing and error handling.
        /// Measures both execution latency and queue time for complete performance analysis.
        /// Reports results to the result collector for aggregation and final metrics calculation.
        /// </summary>
        /// <param name="workItem">The work item to process containing timing and identification data</param>
        /// <param name="workerId">Identifier for the worker processing this item for logging</param>
        private async Task ProcessSingleWorkItem(WorkItem workItem, int workerId)
        {
            try
            {
                // Notify result collector that a new request is starting execution
                // This is used for tracking in-flight requests and calculating accurate throughput
                _resultCollector.Tell(new RequestStartedMessage());

                // Create high-precision stopwatch for accurate execution latency measurement
                // Stopwatch provides the most accurate timing available on the platform
                var stopwatch = Stopwatch.StartNew();
                
                // Execute the actual test action and capture the result
                // This is the user-defined action that represents the workload being tested
                var result = await _executionPlan.Action();
                
                // Stop timing immediately after action completion for accuracy
                // Minimizes measurement overhead in the latency calculation
                stopwatch.Stop();

                // Calculate queue time - the duration between scheduling and actual execution
                // This metric helps identify channel saturation and worker pool adequacy
                var queueTime = (DateTime.UtcNow - workItem.ScheduledTime).TotalMilliseconds;
                
                // Send comprehensive result data including execution latency and queue time
                // This data is aggregated for final performance metrics and percentile calculations
                _resultCollector.Tell(new StepResultMessage(result, stopwatch.Elapsed.TotalMilliseconds, queueTime));

                // Log warning if queue time exceeds threshold (1 second), indicating potential bottleneck
                // High queue times suggest the worker pool may be undersized for the workload
                if (queueTime > 1000)
                {
                    _logger.Warning("Worker {0}: High queue time {1:F2}ms for work item from batch {2}", 
                        workerId, queueTime, workItem.BatchNumber);
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during test action execution
                // Report as failure with zero latency since timing is not meaningful for failed operations
                _resultCollector.Tell(new StepResultMessage(false, 0));
                
                // Log the error with context information for debugging and monitoring
                // Includes worker ID and batch number to help identify patterns or problematic scenarios
                _logger.Error(ex, "Worker {0}: Failed to process work item from batch {1}", 
                    workerId, workItem.BatchNumber);
            }
        }

        /// <summary>
        /// Actor lifecycle method called when the actor is stopping.
        /// Ensures graceful shutdown of all worker threads and proper resource cleanup.
        /// Attempts to complete all in-progress work before terminating.
        /// </summary>
        protected override void PostStop()
        {
            // Cancel all worker operations to begin graceful shutdown
            // This signals all workers to complete their current item and exit
            _workerCts.Cancel();
            
            // Complete the channel to prevent new work items from being added
            // This ensures workers will finish processing existing items and exit
            _workChannel.Writer.TryComplete();
            
            try
            {
                // Wait for all worker tasks to complete with a reasonable timeout
                // This ensures proper cleanup while preventing indefinite hanging
                Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore timeout exceptions during shutdown to prevent blocking actor termination
                // Workers that don't complete in time will be forcefully terminated
            }

            // Dispose the cancellation token source to release resources
            // This is important for preventing resource leaks in long-running systems
            _workerCts.Dispose();
            
            // Call base implementation to complete actor shutdown process
            base.PostStop();
        }

        /// <summary>
        /// Internal data structure representing a unit of work to be processed.
        /// Contains identification, timing, and batch information for comprehensive tracking.
        /// Used for queue time calculation and performance analysis.
        /// </summary>
        private class WorkItem
        {
            /// <summary>
            /// Unique identifier for this work item for tracking and debugging purposes.
            /// Allows correlation of work items across scheduling and processing phases.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// The batch number this work item belongs to for performance analysis.
            /// Enables batch-level metrics and helps identify timing patterns.
            /// </summary>
            public int BatchNumber { get; set; }

            /// <summary>
            /// The precise time when this work item was scheduled for execution.
            /// Used to calculate queue time and identify processing delays.
            /// </summary>
            public DateTime ScheduledTime { get; set; }
        }
    }
}
