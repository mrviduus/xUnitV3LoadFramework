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
        
        /// <summary>
        /// Gets or sets the maximum time to wait for in-flight requests after test duration expires.
        /// Default behavior maintains backward compatibility.
        /// Industry standard: 10-30% of test duration with reasonable bounds.
        /// Null value triggers automatic calculation based on test duration.
        /// </summary>
        public TimeSpan? GracefulStopTimeout { get; set; }
        
        /// <summary>
        /// Gets or sets how the test determines when to stop creating new batches.
        /// Default maintains current behavior for backward compatibility.
        /// Industry standard is CompleteCurrentInterval for predictable request counts.
        /// </summary>
        public TerminationMode TerminationMode { get; set; } = TerminationMode.Duration;
        
        /// <summary>
        /// Gets the effective graceful stop timeout, applying defaults if not specified.
        /// Implements industry standard calculations when null.
        /// Uses 30% of test duration, bounded between 5 seconds and 60 seconds.
        /// </summary>
        public TimeSpan EffectiveGracefulStopTimeout => 
            GracefulStopTimeout ?? CalculateDefaultGracefulStopTimeout();
        
        /// <summary>
        /// Calculates industry-standard graceful stop timeout based on test duration.
        /// Uses 30% of test duration, bounded between 5 seconds and 60 seconds.
        /// Provides reasonable defaults for various test scenarios.
        /// </summary>
        private TimeSpan CalculateDefaultGracefulStopTimeout()
        {
            var thirtyPercentOfDuration = TimeSpan.FromMilliseconds(Duration.TotalMilliseconds * 0.3);
            var minTimeout = TimeSpan.FromSeconds(5);
            var maxTimeout = TimeSpan.FromSeconds(60);
            
            if (thirtyPercentOfDuration < minTimeout)
                return minTimeout;
            if (thirtyPercentOfDuration > maxTimeout)
                return maxTimeout;
                
            return thirtyPercentOfDuration;
        }
    }
}