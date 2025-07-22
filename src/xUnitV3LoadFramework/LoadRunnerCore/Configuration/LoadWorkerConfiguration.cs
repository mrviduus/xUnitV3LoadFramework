namespace xUnitV3LoadFramework.LoadRunnerCore.Configuration
{
    /// <summary>
    /// Configuration settings for load worker actor behavior
    /// </summary>
    public class LoadWorkerConfiguration
    {
        /// <summary>
        /// Determines which implementation of LoadWorkerActor to use
        /// </summary>
        public LoadWorkerMode Mode { get; set; } = LoadWorkerMode.Hybrid;

        /// <summary>
        /// Maximum number of worker threads for hybrid mode
        /// </summary>
        public int? MaxWorkerThreads { get; set; }

        /// <summary>
        /// Channel capacity for hybrid mode (null = unbounded)
        /// </summary>
        public int? ChannelCapacity { get; set; }

        /// <summary>
        /// Enable detailed performance metrics
        /// </summary>
        public bool EnableDetailedMetrics { get; set; } = false;

        /// <summary>
        /// Worker utilization threshold for logging warnings (0.0 to 1.0)
        /// </summary>
        public double WorkerUtilizationWarningThreshold { get; set; } = 0.8;

        /// <summary>
        /// Queue time threshold for logging warnings (milliseconds)
        /// </summary>
        public double QueueTimeWarningThreshold { get; set; } = 1000;
    }

    /// <summary>
    /// Available load worker implementation modes
    /// </summary>
    public enum LoadWorkerMode
    {
        /// <summary>
        /// Original task-based implementation (good for < 10k concurrent requests)
        /// </summary>
        TaskBased,

        /// <summary>
        /// Pure actor-based implementation (good for isolated, supervised scenarios)
        /// </summary>
        ActorBased,

        /// <summary>
        /// Hybrid channel-based implementation (optimal for > 10k concurrent requests)
        /// </summary>
        Hybrid
    }
}
