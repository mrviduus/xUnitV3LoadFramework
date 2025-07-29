// Define namespace for load worker configuration and execution mode settings
// Contains all configuration options that control worker behavior and performance characteristics
namespace xUnitV3LoadFramework.LoadRunnerCore.Configuration
{
    /// <summary>
    /// Configuration settings that control load worker actor behavior and performance characteristics.
    /// Provides fine-grained control over execution modes, resource utilization, and monitoring thresholds.
    /// These settings allow optimization for different load testing scenarios and system constraints.
    /// Enables tuning of worker pools, channels, and performance monitoring for optimal results.
    /// </summary>
    public class LoadWorkerConfiguration
    {
        /// <summary>
        /// Determines which implementation of LoadWorkerActor to use for test execution.
        /// Different modes provide varying performance characteristics and resource usage patterns.
        /// Hybrid mode is default and recommended for most scenarios due to superior scalability.
        /// Selection should be based on expected concurrency levels and system constraints.
        /// </summary>
        public LoadWorkerMode Mode { get; set; } = LoadWorkerMode.Hybrid;

        /// <summary>
        /// Maximum number of worker threads for hybrid mode execution.
        /// Controls the size of the fixed worker pool for optimal resource utilization.
        /// Null value allows automatic calculation based on system resources and concurrency.
        /// Should be tuned based on CPU cores, expected I/O patterns, and memory constraints.
        /// </summary>
        public int? MaxWorkerThreads { get; set; }

        /// <summary>
        /// Channel capacity for hybrid mode work item distribution.
        /// Null value creates unbounded channels for maximum throughput with higher memory usage.
        /// Bounded channels provide backpressure but may limit peak performance under extreme load.
        /// Should be set based on memory constraints and expected burst capacity requirements.
        /// </summary>
        public int? ChannelCapacity { get; set; }

        /// <summary>
        /// Enable detailed performance metrics collection and logging.
        /// Provides comprehensive monitoring data but may impact performance under extreme load.
        /// Useful for performance analysis and troubleshooting but should be disabled for production benchmarks.
        /// Includes per-worker statistics, queue times, and resource utilization tracking.
        /// </summary>
        public bool EnableDetailedMetrics { get; set; } = false;

        /// <summary>
        /// Worker utilization threshold for logging warnings (0.0 to 1.0).
        /// Triggers warnings when worker efficiency falls below this percentage.
        /// Helps identify resource contention, inadequate worker pools, or system bottlenecks.
        /// Values above 0.8 (80%) indicate healthy utilization without resource starvation.
        /// </summary>
        public double WorkerUtilizationWarningThreshold { get; set; } = 0.8;

        /// <summary>
        /// Queue time threshold for logging warnings (milliseconds).
        /// Triggers warnings when work items wait longer than this duration before processing.
        /// High queue times indicate worker pool saturation or insufficient parallel capacity.
        /// Default of 1000ms (1 second) is appropriate for most load testing scenarios.
        /// </summary>
        public double QueueTimeWarningThreshold { get; set; } = 1000;
    }

    /// <summary>
    /// Available load worker implementation modes with different performance characteristics.
    /// Each mode is optimized for specific concurrency levels and resource constraints.
    /// Selection should be based on expected load patterns and system capabilities.
    /// </summary>
    public enum LoadWorkerMode
    {
        /// <summary>
        /// Original task-based implementation using .NET Task.Run for concurrent execution.
        /// Good for moderate load scenarios (< 10k concurrent requests) with standard thread pool management.
        /// Provides simple execution model with good compatibility but limited scalability.
        /// Recommended for functional testing and moderate performance testing scenarios.
        /// </summary>
        TaskBased,

        /// <summary>
        /// Pure actor-based implementation for isolated, supervised execution scenarios.
        /// Good for fault tolerance requirements and distributed testing architectures.
        /// Provides strong isolation and supervision but may have higher overhead.
        /// Recommended when actor supervision and fault recovery are primary concerns.
        /// </summary>
        ActorBased,

        /// <summary>
        /// Hybrid channel-based implementation optimized for high-throughput scenarios.
        /// Optimal for high concurrency load testing (> 10k concurrent requests) with minimal overhead.
        /// Uses fixed worker pools with high-performance channels for maximum scalability.
        /// Recommended for stress testing, capacity planning, and performance benchmarking.
        /// </summary>
        Hybrid
    }
}
