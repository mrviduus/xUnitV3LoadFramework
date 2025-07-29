namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    /// <summary>
    /// Message containing information about worker thread utilization.
    /// Used for monitoring resource usage and optimizing thread pool configuration.
    /// </summary>
    public class WorkerThreadCountMessage
    {
        /// <summary>
        /// Gets or sets the number of worker threads currently active.
        /// Used for tracking resource utilization and system capacity.
        /// </summary>
        public int ThreadCount { get; set; }
    }
}
