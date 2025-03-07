using System;

namespace LoadTestRunner.Models
{
    public class LoadExecutionSettings
    {
        public int Concurrency { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }
    }
}