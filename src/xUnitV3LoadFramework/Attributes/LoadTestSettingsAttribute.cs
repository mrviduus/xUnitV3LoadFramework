namespace xUnitV3LoadFramework.Attributes
{
    //[AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadTestSettingsAttribute : Attribute
    {
        public int Concurrency { get; set; }
        public int DurationInSeconds { get; set; }
        public int IntervalInSeconds { get; set; }

        public LoadTestSettingsAttribute(int concurrency = 1, int durationInSeconds = 1, int intervalInSeconds = 1)
        {
            if (concurrency < 1)
                throw new ArgumentOutOfRangeException(nameof(concurrency), "Concurrency must be at least 1.");
            if (durationInSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(durationInSeconds), "Duration must be at least 1 second.");
            if (intervalInSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(intervalInSeconds), "Interval must be at least 1 second.");

            Concurrency = concurrency;
            DurationInSeconds = durationInSeconds;
            IntervalInSeconds = intervalInSeconds;
        }
    }
}