using System;

namespace xUnitLoadRunner
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LoadTestSettingsAttribute : Attribute
    {
        public int Concurrency { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }

        public LoadTestSettingsAttribute(int concurrency = 1, TimeSpan duration = default, TimeSpan interval = default)
        {
            Concurrency = concurrency;
            Duration = duration;
            Interval = interval;
        }
    }
}
