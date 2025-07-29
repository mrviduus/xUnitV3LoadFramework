namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    /// <summary>
    /// Message indicating that a batch of load test operations has completed execution.
    /// Provides batch-level metrics for monitoring test progress and performance analysis.
    /// </summary>
    public class BatchCompletedMessage
    {
        /// <summary>
        /// Gets the sequential number of the completed batch.
        /// Used for tracking test progress and execution ordering.
        /// </summary>
        public int BatchNumber { get; }
        
        /// <summary>
        /// Gets the number of individual items processed in this batch.
        /// Indicates the batch size and processing efficiency.
        /// </summary>
        public int ItemsProcessed { get; }
        
        /// <summary>
        /// Gets the timestamp when the batch completed execution.
        /// Used for calculating batch processing times and throughput metrics.
        /// </summary>
        public DateTime CompletedAt { get; }

        /// <summary>
        /// Initializes a new instance of the BatchCompletedMessage class with explicit timestamp.
        /// </summary>
        /// <param name="batchNumber">The sequential batch number</param>
        /// <param name="itemsProcessed">Number of items processed in the batch</param>
        /// <param name="completedAt">Timestamp when the batch completed</param>
        public BatchCompletedMessage(int batchNumber, int itemsProcessed, DateTime completedAt)
        {
            // Store the batch identifier for tracking purposes
            BatchNumber = batchNumber;
            // Record the number of processed items for metrics calculation
            ItemsProcessed = itemsProcessed;
            // Capture the completion timestamp for timing analysis
            CompletedAt = completedAt;
        }

        /// <summary>
        /// Initializes a new instance of the BatchCompletedMessage class with current timestamp.
        /// </summary>
        /// <param name="batchNumber">The sequential batch number</param>
        /// <param name="itemsProcessed">Number of items processed in the batch</param>
        public BatchCompletedMessage(int batchNumber, int itemsProcessed) 
            : this(batchNumber, itemsProcessed, DateTime.UtcNow)
        {
            // Delegate to the main constructor with current UTC timestamp
        }
    }
}
