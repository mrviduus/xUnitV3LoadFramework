// Import xUnit framework for test attribute inheritance and integration
using System.Runtime.CompilerServices;
using Xunit;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Attribute that marks a test method for stress testing execution with specified parameters.
/// Inherits from FactAttribute to integrate with xUnit test discovery and execution.
/// Provides configuration for concurrency, duration, execution order, and timing intervals.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class StressAttribute : FactAttribute
{
	/// <summary>
	/// Initializes a new instance of the StressAttribute class with stress test parameters.
	/// </summary>
	/// <param name="order">Execution order for sequencing multiple stress tests</param>
	/// <param name="concurrency">Number of concurrent executions to run simultaneously</param>
	/// <param name="duration">Total duration for the stress test in milliseconds</param>
	/// <param name="interval">Time interval between batches in milliseconds</param>
	/// <param name="sourceFilePath">Source file path (automatically provided by compiler)</param>
	/// <param name="sourceLineNumber">Source line number (automatically provided by compiler)</param>
	public StressAttribute(
		int order, 
		int concurrency, 
		int duration, 
		int interval,
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0) : base(sourceFilePath, sourceLineNumber)
	{
		// Set the execution order for test sequencing
		Order = order;
		// Initialize stress settings with validated parameters
		_settings = new StressSettings
		{
			// Configure concurrent execution count
			Concurrency = concurrency,
			// Set total test duration in milliseconds
			Duration = duration,
			// Define interval between execution batches
			Interval = interval
		};
	}

	/// <summary>
	/// Gets or sets the execution order of the stress test.
	/// Lower values execute first, enabling sequential test execution.
	/// </summary>
	public int Order { get; set; } = 0;

	// Private field to store validated stress test settings
	private StressSettings _settings;

	/// <summary>
	/// Gets or sets the number of concurrent test executions.
	/// Determines how many operations run simultaneously during stress testing.
	/// </summary>
	public int Concurrency
	{
		// Return the current concurrency setting from internal storage
		get => _settings.Concurrency;
		// Update concurrency with validation through StressSettings
		set => _settings.Concurrency = value;
	}

	/// <summary>
	/// Gets or sets the duration of the stress test in milliseconds.
	/// Defines how long the stress test will continue executing operations.
	/// </summary>
	public int Duration
	{
		// Return the current duration setting from internal storage
		get => _settings.Duration;
		// Update duration with validation through StressSettings
		set => _settings.Duration = value;
	}

	/// <summary>
	/// Gets or sets the interval between each batch of concurrent executions in milliseconds.
	/// Controls the rate at which new batches of operations are initiated.
	/// </summary>
	public int Interval
	{
		// Return the current interval setting from internal storage
		get => _settings.Interval;
		// Update interval with validation through StressSettings
		set => _settings.Interval = value;
	}

	/// <summary>
	/// Internal structure to store and validate stress test configuration parameters.
	/// Ensures all settings meet minimum requirements for valid stress test execution.
	/// </summary>
	private struct StressSettings
	{
		// Private field to store validated concurrency value
		private int _concurrency;
		// Private field to store validated duration value
		private int _duration;
		// Private field to store validated interval value
		private int _interval;

		/// <summary>
		/// Gets or sets the number of concurrent executions with validation.
		/// Ensures concurrency is at least 1 for meaningful stress testing.
		/// </summary>
		public int Concurrency
		{
			// Return the stored concurrency value
			get => _concurrency;
			// Validate and set concurrency with range checking
			set => _concurrency = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Concurrency), "Concurrency must be at least 1.")
				: value;
		}

		/// <summary>
		/// Gets or sets the test duration with validation.
		/// Ensures duration is at least 1 millisecond for valid execution timing.
		/// </summary>
		public int Duration
		{
			// Return the stored duration value
			get => _duration;
			// Validate and set duration with minimum value checking
			set => _duration = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be at least 1 second.")
				: value;
		}

		/// <summary>
		/// Gets or sets the batch interval with validation.
		/// Ensures interval is at least 1 millisecond for proper batch timing.
		/// </summary>
		public int Interval
		{
			// Return the stored interval value
			get => _interval;
			// Validate and set interval with minimum value checking
			set => _interval = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Interval), "Interval must be at least 1 second.")
				: value;
		}
	}
}
