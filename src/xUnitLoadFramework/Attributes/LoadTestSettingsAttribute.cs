namespace xUnitLoadFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadTestSettingsAttribute : Attribute
    {
        public int Concurrency { get; set; }
        public int DurationInMilliseconds { get; set; }
        public int IntervalInMilliseconds { get; set; }

        public LoadTestSettingsAttribute(int concurrency = 1, int durationInMilliseconds = 1, int intervalInMilliseconds = 1)
        {
            if (concurrency < 1)
                throw new ArgumentOutOfRangeException(nameof(concurrency), "Concurrency must be at least 1.");
            if (durationInMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(durationInMilliseconds), "Duration must be at least 1 second.");
            if (intervalInMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(intervalInMilliseconds), "Interval must be at least 1 second.");

            Concurrency = concurrency;
            DurationInMilliseconds = durationInMilliseconds;
            IntervalInMilliseconds = intervalInMilliseconds;
        }
    }
}