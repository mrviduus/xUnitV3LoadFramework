namespace xUnitLoadFramework.Extensions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ObservationAttribute : Attribute
{
    public int Order { get; set; } = 0;
}
