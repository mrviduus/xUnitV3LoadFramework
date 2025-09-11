// Define namespace for load testing configuration models and settings structures
// Contains all configuration DTOs that control load test execution behavior
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    /// <summary>
    /// Defines how the load test determines when to stop creating new batches.
    /// Controls the timing precision and request count accuracy of load tests.
    /// </summary>
    public enum TerminationMode
    {
        /// <summary>
        /// Stop immediately when duration is reached (current behavior).
        /// Maintains backward compatibility with existing tests.
        /// May result in fewer requests than theoretically expected.
        /// </summary>
        Duration,
        
        /// <summary>
        /// Complete the current interval before stopping (industry standard).
        /// Ensures more predictable request counts.
        /// May run slightly longer than specified duration.
        /// </summary>
        CompleteCurrentInterval,
        
        /// <summary>
        /// Run for exactly the specified duration, cutting off final batch if needed.
        /// Prioritizes strict timing over request count consistency.
        /// Useful for time-sensitive testing scenarios.
        /// </summary>
        StrictDuration
    }
}
