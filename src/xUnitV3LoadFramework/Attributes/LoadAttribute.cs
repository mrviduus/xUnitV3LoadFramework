using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace xUnitV3LoadFramework.Attributes
{
    /// <summary>
    /// Attribute that marks a test method for load testing execution with specified parameters.
    /// Inherits from FactAttribute to integrate with xUnit v3 test discovery and execution.
    /// The load testing logic is handled separately from standard xUnit execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LoadAttribute : FactAttribute
    {
        /// <summary>
        /// Gets the number of concurrent executions for this load test.
        /// </summary>
        public int Concurrency { get; }

        /// <summary>
        /// Gets the duration of the load test in milliseconds.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// Gets the interval between batches in milliseconds.
        /// </summary>
        public int Interval { get; }

        /// <summary>
        /// Initializes a new instance of the LoadAttribute class with load test parameters.
        /// </summary>
        /// <param name="concurrency">Number of concurrent executions to run simultaneously</param>
        /// <param name="duration">Total duration for the load test in milliseconds</param>
        /// <param name="interval">Time interval between batches in milliseconds</param>
        /// <param name="sourceFilePath">Source file path (automatically provided by compiler)</param>
        /// <param name="sourceLineNumber">Source line number (automatically provided by compiler)</param>
        public LoadAttribute(
            int concurrency,
            int duration,
            int interval,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) : base(sourceFilePath, sourceLineNumber)
        {
            Concurrency = concurrency;
            Duration = duration;
            Interval = interval;
        }
    }
}
