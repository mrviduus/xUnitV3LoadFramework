using System.Diagnostics;
using System.Threading.Channels;
using Akka.Actor;
using Akka.Event;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    /// <summary>
    /// Hybrid implementation of LoadWorkerActor that uses a fixed pool of worker tasks
    /// with channels for efficient work distribution. This approach prevents thread pool
    /// exhaustion and provides better scalability for high-load scenarios (100k+ requests).
    /// </summary>
    public class LoadWorkerActorHybrid : ReceiveActor
    {
        private readonly LoadExecutionPlan _executionPlan;
        private readonly IActorRef _resultCollector;
        private readonly ILoggingAdapter _logger = Context.GetLogger();
        private readonly Channel<WorkItem> _workChannel;
        private readonly List<Task> _workerTasks;
        private readonly CancellationTokenSource _workerCts = new();
        private readonly int _workerCount;

        public LoadWorkerActorHybrid(LoadExecutionPlan executionPlan, IActorRef resultCollector)
        {
            _executionPlan = executionPlan;
            _resultCollector = resultCollector;

            // Create unbounded channel for work distribution
            _workChannel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            // Calculate optimal worker count based on system resources and concurrency
            _workerCount = CalculateOptimalWorkerCount(executionPlan.Settings.Concurrency);
            _workerTasks = new List<Task>(_workerCount);

            // Define message handlers
            ReceiveAsync<StartLoadMessage>(async _ => await RunWorkAsync());
        }

        private int CalculateOptimalWorkerCount(int concurrency)
        {
            // Use a formula that considers both CPU cores and expected concurrency
            var coreCount = Environment.ProcessorCount;
            var baseWorkers = coreCount * 2; // 2 workers per core for I/O bound tasks
            
            // Scale workers based on concurrency, but cap at reasonable limits
            var scaledWorkers = Math.Max(baseWorkers, concurrency / 10);
            var maxWorkers = Math.Min(1000, coreCount * 50); // Cap at 50x cores or 1000
            
            var optimalWorkers = Math.Min(scaledWorkers, maxWorkers);
            
            _logger.Info("Calculated optimal worker count: {0} (cores: {1}, concurrency: {2})", 
                optimalWorkers, coreCount, concurrency);
            
            return optimalWorkers;
        }

        private async Task RunWorkAsync()
        {
            var workerName = Self.Path.Name;
            _resultCollector.Tell(new StartLoadMessage());
            _resultCollector.Tell(new WorkerThreadCountMessage { ThreadCount = _workerCount });
            
            using var cts = new CancellationTokenSource(_executionPlan.Settings.Duration);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _workerCts.Token);

            try
            {
                // Start worker tasks
                for (int i = 0; i < _workerCount; i++)
                {
                    var workerId = i;
                    _workerTasks.Add(ProcessWorkItems(workerId, linkedCts.Token));
                }

                // Start scheduling work items
                var schedulerTask = ScheduleWorkItems(linkedCts.Token);

                // Wait for duration to complete
                await Task.Delay(_executionPlan.Settings.Duration, linkedCts.Token);

                // Signal completion
                _workChannel.Writer.TryComplete();

                // Wait for all workers to finish processing remaining items
                await Task.WhenAll(_workerTasks);
                await schedulerTask;
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("LoadWorkerActorHybrid '{0}' operation cancelled", workerName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "LoadWorkerActorHybrid '{0}' encountered an error", workerName);
            }
            finally
            {
                // Ensure channel is completed
                _workChannel.Writer.TryComplete();

                // Get final results
                var finalResult = await _resultCollector.Ask<LoadResult>(
                    new GetLoadResultMessage(), TimeSpan.FromSeconds(5));
                
                _logger.Info("LoadWorkerActorHybrid '{0}' completed. Total: {1}, Success: {2}, Failed: {3}, In-flight: {4}", 
                    workerName, finalResult.Total, finalResult.Success, finalResult.Failure, finalResult.RequestsInFlight);
                
                Sender.Tell(finalResult);
            }
        }

        private async Task ScheduleWorkItems(CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            var batchNumber = 0;
            var totalScheduled = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsedTime = DateTime.UtcNow - startTime;
                
                if (elapsedTime >= _executionPlan.Settings.Duration)
                    break;

                // Schedule batch of work items
                for (int i = 0; i < _executionPlan.Settings.Concurrency; i++)
                {
                    var workItem = new WorkItem
                    {
                        Id = Guid.NewGuid(),
                        BatchNumber = batchNumber,
                        ScheduledTime = DateTime.UtcNow
                    };

                    await _workChannel.Writer.WriteAsync(workItem, cancellationToken);
                    totalScheduled++;
                }

                _logger.Debug("Batch {0} scheduled with {1} items. Total scheduled: {2}", 
                    batchNumber, _executionPlan.Settings.Concurrency, totalScheduled);

                // Notify that batch is completed
                _resultCollector.Tell(new BatchCompletedMessage(batchNumber, _executionPlan.Settings.Concurrency, DateTime.UtcNow));

                batchNumber++;

                // Calculate next batch time
                var nextBatchTime = startTime.AddMilliseconds(batchNumber * _executionPlan.Settings.Interval.TotalMilliseconds);
                var delay = nextBatchTime - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            _logger.Info("Work scheduling completed. Total items scheduled: {0}", totalScheduled);
        }

        private async Task ProcessWorkItems(int workerId, CancellationToken cancellationToken)
        {
            var processedCount = 0;

            try
            {
                await foreach (var workItem in _workChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    await ProcessSingleWorkItem(workItem, workerId);
                    processedCount++;
                    
                    // Yield periodically to prevent blocking
                    if (processedCount % 100 == 0)
                    {
                        await Task.Yield();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Worker {0} cancelled after processing {1} items", workerId, processedCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Worker {0} encountered error after processing {1} items", workerId, processedCount);
            }

            _logger.Debug("Worker {0} completed. Processed {1} items", workerId, processedCount);
        }

        private async Task ProcessSingleWorkItem(WorkItem workItem, int workerId)
        {
            try
            {
                _resultCollector.Tell(new RequestStartedMessage());

                var stopwatch = Stopwatch.StartNew();
                var result = await _executionPlan.Action();
                stopwatch.Stop();

                var queueTime = (DateTime.UtcNow - workItem.ScheduledTime).TotalMilliseconds;
                _resultCollector.Tell(new StepResultMessage(result, stopwatch.Elapsed.TotalMilliseconds, queueTime));

                if (queueTime > 1000) // Log if queue time exceeds 1 second
                {
                    _logger.Warning("Worker {0}: High queue time {1:F2}ms for work item from batch {2}", 
                        workerId, queueTime, workItem.BatchNumber);
                }
            }
            catch (Exception ex)
            {
                _resultCollector.Tell(new StepResultMessage(false, 0));
                _logger.Error(ex, "Worker {0}: Failed to process work item from batch {1}", 
                    workerId, workItem.BatchNumber);
            }
        }

        protected override void PostStop()
        {
            _workerCts.Cancel();
            _workChannel.Writer.TryComplete();
            
            try
            {
                Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore timeout exceptions during shutdown
            }

            _workerCts.Dispose();
            base.PostStop();
        }

        private class WorkItem
        {
            public Guid Id { get; set; }
            public int BatchNumber { get; set; }
            public DateTime ScheduledTime { get; set; }
        }
    }
}
