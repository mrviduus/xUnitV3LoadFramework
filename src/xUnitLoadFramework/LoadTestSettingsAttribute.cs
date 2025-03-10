using System;

namespace xUnitLoadRunner
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LoadTestSettingsAttribute : Attribute
    {
        public int Concurrency { get; set; }
        public int DurationInSeconds { get; set; }
        public int IntervalInSeconds { get; set; }

        public LoadTestSettingsAttribute(int concurrency = 1, int durationInSeconds = 1, int intervalInSeconds = 1)
        {
            Concurrency = concurrency;
            DurationInSeconds = durationInSeconds;
            IntervalInSeconds = intervalInSeconds;
        }
    }
}
