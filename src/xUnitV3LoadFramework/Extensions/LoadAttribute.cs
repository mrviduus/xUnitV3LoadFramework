namespace xUnitV3LoadFramework.Extensions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LoadAttribute : Attribute
{
    public int Order { get; set; } = 0;
}
