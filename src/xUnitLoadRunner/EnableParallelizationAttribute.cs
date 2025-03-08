using System;

namespace xUnitLoadRunner;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class EnableParallelizationAttribute : Attribute
{
}
