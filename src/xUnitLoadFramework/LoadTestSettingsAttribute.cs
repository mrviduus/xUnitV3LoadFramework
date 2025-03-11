using System;

namespace xUnitLoadFramework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LoadTestSettingsAttribute(
        int concurrency = 1,
        int durationInSeconds = 1,
        int intervalInSeconds = 1)
        : Attribute
    {
        public int Concurrency { get; set; } = concurrency;
        public int DurationInSeconds { get; set; } = durationInSeconds;
        public int IntervalInSeconds { get; set; } = intervalInSeconds;
    }
}
