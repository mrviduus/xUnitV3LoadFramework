namespace ObservationExample;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LoadAttribute : Attribute
{
	public int Order { get; set; } = 0;
}
