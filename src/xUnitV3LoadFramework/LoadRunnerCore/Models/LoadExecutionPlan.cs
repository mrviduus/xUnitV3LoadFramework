// Define namespace for load testing data models and configuration structures
// Contains all DTOs and data contracts used throughout the load testing framework
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    /// <summary>
    /// Defines a complete load test execution plan containing test configuration and action.
    /// This class encapsulates all information needed to execute a load test scenario.
    /// Serves as the primary contract between test definition and execution infrastructure.
    /// Immutable configuration that drives all aspects of load test execution.
    /// </summary>
    public class LoadExecutionPlan
    {
        /// <summary>
        /// Gets or sets the unique name identifier for this load test scenario.
        /// Used for logging, reporting, and result identification purposes.
        /// This name appears in all log messages and result files for traceability.
        /// Should be descriptive and unique within the test suite for clarity.
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the load test configuration settings including concurrency, duration, and intervals.
        /// Defines how the load test should be executed in terms of timing and scale.
        /// Contains all parameters that control the test execution pattern and resource utilization.
        /// This configuration drives the worker creation and scheduling algorithms.
        /// </summary>
        public required LoadSettings Settings { get; set; }
        
        /// <summary>
        /// Gets or sets the asynchronous test action to be executed during load testing.
        /// Returns true for successful execution, false for failure - used for success rate calculations.
        /// This function represents the actual workload that will be subjected to load testing.
        /// Should be idempotent and thread-safe as it will be executed concurrently by multiple workers.
        /// Performance of this action directly impacts the overall test results and metrics.
        /// </summary>
        public required Func<Task<bool>> Action { get; set; }
    }
}