// Define namespace for load testing configuration models and settings structures
// Contains all configuration DTOs that control load test execution behavior
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    /// <summary>
    /// Configuration settings that define the execution parameters for load testing scenarios.
    /// Controls the timing, concurrency, and pattern of load generation during test execution.
    /// These settings directly impact resource utilization and test accuracy.
    /// Immutable configuration that drives worker scheduling and result interpretation.
    /// </summary>
    public class LoadSettings
    {
        /// <summary>
        /// Gets or sets the number of concurrent operations to execute in each batch interval.
        /// This controls the parallel load applied to the system under test.
        /// Higher values increase system stress but require more resources.
        /// Must be balanced with system capacity and test objectives.
        /// Should be tuned based on expected system throughput and resource constraints.
        /// </summary>
        public int Concurrency { get; set; }
        
        /// <summary>
        /// Gets or sets the total duration for which the load test should execute.
        /// Defines the overall time window during which load will be generated.
        /// Longer durations provide more stable metrics but consume more resources.
        /// Should be sufficient to reach steady-state performance characteristics.
        /// Minimum recommended duration is 30 seconds for meaningful statistical analysis.
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the interval between batch executions for consistent load patterns.
        /// Controls the timing between successive waves of concurrent operations.
        /// Shorter intervals create more sustained load but may cause resource contention.
        /// Should be tuned based on expected system response times and throughput goals.
        /// Common values range from 100ms for high-frequency tests to 5000ms for periodic load.
        /// </summary>
        public TimeSpan Interval { get; set; }
    }
}