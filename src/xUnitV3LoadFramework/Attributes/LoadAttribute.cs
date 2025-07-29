// Import xUnit framework for test attribute inheritance and integration
using Xunit;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Attribute that marks a test method for load testing execution with specified parameters.
/// Inherits from FactAttribute to integrate with xUnit test discovery and execution.
/// Provides configuration for concurrency, duration, execution order, and timing intervals.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LoadAttribute : FactAttribute
{
	/// <summary>
	/// Initializes a new instance of the LoadAttribute class with load test parameters.
	/// </summary>
	/// <param name="order">Execution order for sequencing multiple load tests</param>
	/// <param name="concurrency">Number of concurrent executions to run simultaneously</param>
	/// <param name="duration">Total duration for the load test in milliseconds</param>
	/// <param name="interval">Time interval between batches in milliseconds</param>
	public LoadAttribute(int order, int concurrency, int duration, int interval)
	{
		// Set the execution order for test sequencing
		Order = order;
		// Initialize load settings with validated parameters
		_settings = new LoadSettings
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
	/// Gets or sets the execution order of the load test.
	/// Lower values execute first, enabling sequential test execution.
	/// </summary>
	public int Order { get; set; } = 0;

	// Private field to store validated load test settings
	private LoadSettings _settings;

	/// <summary>
	/// Gets or sets the number of concurrent test executions.
	/// Determines how many operations run simultaneously during load testing.
	/// </summary>
	public int Concurrency
	{
		// Return the current concurrency setting from internal storage
		get => _settings.Concurrency;
		// Update concurrency with validation through LoadSettings
		set => _settings.Concurrency = value;
	}

	/// <summary>
	/// Gets or sets the duration of the load test in milliseconds.
	/// Defines how long the load test will continue executing operations.
	/// </summary>
	public int Duration
	{
		// Return the current duration setting from internal storage
		get => _settings.Duration;
		// Update duration with validation through LoadSettings
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
		// Update interval with validation through LoadSettings
		set => _settings.Interval = value;
	}

	/// <summary>
	/// Internal structure to store and validate load test configuration parameters.
	/// Ensures all settings meet minimum requirements for valid load test execution.
	/// </summary>
	private struct LoadSettings
	{
		// Private field to store validated concurrency value
		private int _concurrency;
		// Private field to store validated duration value
		private int _duration;
		// Private field to store validated interval value
		private int _interval;

		/// <summary>
		/// Gets or sets the number of concurrent executions with validation.
		/// Ensures concurrency is at least 1 for meaningful load testing.
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
