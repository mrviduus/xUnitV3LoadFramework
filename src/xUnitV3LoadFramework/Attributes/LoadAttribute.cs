// Import xUnit framework for test attribute inheritance and integration
using System.Runtime.CompilerServices;
using Xunit;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Attribute that marks a test method for load testing execution with specified parameters.
/// Inherits from FactAttribute to integrate with xUnit test discovery and execution.
/// Provides configuration for concurrency, duration, execution order, and timing intervals.
/// 
/// DEPRECATED: Use StressAttribute instead. This will be removed in v3.0.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Obsolete("Use StressAttribute instead. LoadAttribute will be removed in v3.0.", false)]
public class LoadAttribute : StressAttribute
{
	/// <summary>
	/// Initializes a new instance of the LoadAttribute class with load test parameters.
	/// </summary>
	/// <param name="order">Execution order for sequencing multiple load tests</param>
	/// <param name="concurrency">Number of concurrent executions to run simultaneously</param>
	/// <param name="duration">Total duration for the load test in milliseconds</param>
	/// <param name="interval">Time interval between batches in milliseconds</param>
	/// <param name="sourceFilePath">Source file path (automatically provided by compiler)</param>
	/// <param name="sourceLineNumber">Source line number (automatically provided by compiler)</param>
	public LoadAttribute(
		int order, 
		int concurrency, 
		int duration, 
		int interval,
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0) : base(order, concurrency, duration, interval, sourceFilePath, sourceLineNumber)
	{
		// All functionality is now handled by the StressAttribute base class
	}
}
