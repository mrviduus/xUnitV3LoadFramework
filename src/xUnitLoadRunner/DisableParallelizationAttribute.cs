using System;

namespace xUnitLoadRunner;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DisableParallelizationAttribute : Attribute
{
}
