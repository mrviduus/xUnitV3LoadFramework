namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    public class BatchCompletedMessage
    {
        public int BatchNumber { get; }
        public int ItemsProcessed { get; }
        public DateTime CompletedAt { get; }

        public BatchCompletedMessage(int batchNumber, int itemsProcessed, DateTime completedAt)
        {
            BatchNumber = batchNumber;
            ItemsProcessed = itemsProcessed;
            CompletedAt = completedAt;
        }

        public BatchCompletedMessage(int batchNumber, int itemsProcessed) 
            : this(batchNumber, itemsProcessed, DateTime.UtcNow)
        {
        }
    }
}
