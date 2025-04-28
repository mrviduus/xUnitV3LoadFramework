namespace xUnitV3LoadFramework.Extensions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LoadAttribute : Attribute
{
	public int Order { get; set; } = 0;
	//private int _concurrency;
	//private int _duration;
	//private int _interval;

	public int Concurrency { get; set; } = 0;
	public int Duration { get; set; } = 0;
	public int Interval { get; set; } = 0;

	//public int Duration
	//{
	//	get => _duration;
	//	set => _duration = value < 1
	//		? throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be at least 1 second.")
	//		: value;
	//}

	//public int Interval
	//{
	//	get => _interval;
	//	set => _interval = value < 1
	//		? throw new ArgumentOutOfRangeException(nameof(Interval), "Interval must be at least 1 second.")
	//		: value;
	//}
}
