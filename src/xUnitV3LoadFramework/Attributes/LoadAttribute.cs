using Xunit;

namespace xUnitV3LoadFramework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LoadAttribute : FactAttribute
{
	public LoadAttribute(int order, int concurrency, int duration, int interval)
	{
		Order = order;
		_settings = new LoadSettings
		{
			Concurrency = concurrency,
			Duration = duration,
			Interval = interval
		};
	}

	/// <summary>
	/// Execution order of the load test.
	/// </summary>
	public int Order { get; set; } = 0;

	private LoadSettings _settings;

	/// <summary>
	/// Number of concurrent test executions.
	/// </summary>
	public int Concurrency
	{
		get => _settings.Concurrency;
		set => _settings.Concurrency = value;
	}

	/// <summary>
	/// Duration of the load test in milliseconds.
	/// </summary>
	public int Duration
	{
		get => _settings.Duration;
		set => _settings.Duration = value;
	}

	/// <summary>
	/// Interval between each batch of concurrent executions in milliseconds.
	/// </summary>
	public int Interval
	{
		get => _settings.Interval;
		set => _settings.Interval = value;
	}

	private struct LoadSettings
	{
		private int _concurrency;
		private int _duration;
		private int _interval;

		public int Concurrency
		{
			get => _concurrency;
			set => _concurrency = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Concurrency), "Concurrency must be at least 1.")
				: value;
		}

		public int Duration
		{
			get => _duration;
			set => _duration = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be at least 1 second.")
				: value;
		}

		public int Interval
		{
			get => _interval;
			set => _interval = value < 1
				? throw new ArgumentOutOfRangeException(nameof(Interval), "Interval must be at least 1 second.")
				: value;
		}
	}
}
