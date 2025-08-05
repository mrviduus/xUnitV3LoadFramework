// Import Akka.NET actor framework for distributed load testing architecture
// Provides the foundation for fault-tolerant, scalable, message-driven actors
using Akka.Actor;
// Import load testing actors for worker and result collection functionality
// Contains specialized actors for executing load tests and aggregating results
using xUnitV3LoadFramework.LoadRunnerCore.Actors;
// Import configuration settings for customizing load worker behavior
// Defines different execution modes and performance tuning options
using xUnitV3LoadFramework.LoadRunnerCore.Configuration;
// Import message types for actor communication and coordination
// Contains all message contracts used for inter-actor communication
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
// Import data models for load test execution plans and results
// Defines the structure for test configuration and result aggregation
using xUnitV3LoadFramework.LoadRunnerCore.Models;

// Define the namespace for load test orchestration and execution
namespace xUnitV3LoadFramework.LoadRunnerCore.Runner
{
	/// <summary>
	/// Main entry point for executing load tests using actor-based architecture.
	/// Orchestrates the creation and coordination of load worker and result collector actors.
	/// Provides both synchronous and asynchronous execution patterns with comprehensive metrics collection.
	/// </summary>
	public static class LoadRunner
	{
		/// <summary>
		/// Executes a load test with default configuration settings.
		/// This is a convenience method that uses the hybrid load worker mode with default settings.
		/// </summary>
		/// <param name="executionPlan">The load test execution plan containing test action and settings</param>
		/// <returns>Aggregated load test results including performance metrics</returns>
		public static async Task<LoadResult> Run(LoadExecutionPlan executionPlan)
		{
			// Delegate to the main Run method with default configuration
			// Uses LoadWorkerConfiguration() which defaults to Hybrid mode for optimal performance
			return await Run(executionPlan, new LoadWorkerConfiguration());
		}

		/// <summary>
		/// Executes a load test with specified configuration settings.
		/// Creates an actor system to manage load workers and result collection.
		/// Supports multiple execution modes for different performance characteristics.
		/// </summary>
		/// <param name="executionPlan">The load test execution plan containing test action and settings</param>
		/// <param name="configuration">Optional configuration for load worker behavior and performance tuning</param>
		/// <returns>Aggregated load test results with detailed performance metrics</returns>
		public static async Task<LoadResult> Run(
			LoadExecutionPlan executionPlan, 
			LoadWorkerConfiguration? configuration = null)
		{
			// Validate that the execution plan contains a valid test action
			// Ensures we have a test action to execute before proceeding
			if (executionPlan.Action == null)
				throw new ArgumentNullException(nameof(executionPlan.Action));

			// Use default configuration if none provided, enabling flexible test execution
			// Default configuration uses Hybrid mode for optimal performance characteristics
			configuration ??= new LoadWorkerConfiguration();

			// Create a new actor system for this load test execution to ensure isolation
			// The actor system manages all actors and provides fault tolerance and supervision
			using var actorSystem = ActorSystem.Create("LoadTestSystem");
			
			// Create a result collector actor to aggregate metrics from all load workers
			// This actor will collect timing, success/failure, and performance data from worker threads
			// Uses the execution plan name for tracking and logging purposes
			var resultCollector = actorSystem.ActorOf(
				Props.Create(() => new ResultCollectorActor(executionPlan.Name)),
				"resultCollector"
			);

			// Create the appropriate load worker actor based on configuration mode
			// Different modes provide varying levels of performance and resource usage
			// Pattern matching ensures type safety and exhaustive mode coverage
			var loadWorkerProps = configuration.Mode switch
			{
				// Task-based mode: Uses .NET Task.Run for concurrent execution
				// Suitable for moderate load scenarios with good thread pool management
				LoadWorkerMode.TaskBased => Props.Create(() => 
					new LoadWorkerActor(executionPlan, resultCollector)),
					
				// Hybrid mode: Uses fixed thread pool with channels for high-performance scenarios
				// Optimized for high-throughput scenarios (100k+ requests) with minimal overhead
				LoadWorkerMode.Hybrid => Props.Create(() => 
					new LoadWorkerActorHybrid(executionPlan, resultCollector)),
					
				// Throw exception for unsupported modes to fail fast during development
				// Ensures all modes are explicitly implemented and prevents silent failures
				_ => throw new ArgumentException($"LoadWorkerMode {configuration.Mode} is not yet implemented")
			};

			// Create and start the load worker actor within the actor system
			// The worker name "worker" provides a consistent identity for logging and monitoring
			var worker = actorSystem.ActorOf(loadWorkerProps, "worker");

			// Send start message to worker and wait for completion with adaptive timeout
			// The timeout uses a more generous buffer to account for system load and actor overhead
			// Minimum timeout of 30 seconds ensures enough time for actor initialization and cleanup
			var workerTimeout = TimeSpan.FromSeconds(Math.Max(30, executionPlan.Settings.Duration.TotalSeconds + 20));
			
			try
			{
				await worker.Ask<LoadResult>(
					new StartLoadMessage(),
					workerTimeout
				);
			}
			catch (TimeoutException ex)
			{
				throw new TimeoutException(
					$"Load test worker timed out after {workerTimeout.TotalSeconds} seconds. " +
					$"Test duration was {executionPlan.Settings.Duration.TotalSeconds} seconds. " +
					$"Consider increasing test timeout or reducing test complexity.", ex);
			}

			// Request final aggregated results from the result collector actor
			// This includes all performance metrics, latency percentiles, and error rates
			// Separate request ensures all data has been processed and aggregated
			var resultTimeout = TimeSpan.FromSeconds(Math.Max(15, executionPlan.Settings.Duration.TotalSeconds + 10));
			
			try
			{
				var finalResult = await resultCollector.Ask<LoadResult>(
					new GetLoadResultMessage(),
					resultTimeout
				);

				// Return the comprehensive load test results for analysis and reporting
				// Results include timing data, throughput metrics, error rates, and resource utilization
				return finalResult;
			}
			catch (TimeoutException ex)
			{
				throw new TimeoutException(
					$"Result collector timed out after {resultTimeout.TotalSeconds} seconds. " +
					$"This may indicate issues with result aggregation or actor communication.", ex);
			}
		}
	}
}