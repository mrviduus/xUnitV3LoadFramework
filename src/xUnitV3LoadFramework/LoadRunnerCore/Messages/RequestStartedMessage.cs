namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    /// <summary>
    /// Message indicating that a load test request has been initiated.
    /// Used for tracking request lifecycle and monitoring test execution flow.
    /// </summary>
    public class RequestStartedMessage
    {
        /// <summary>
        /// Gets the timestamp when the request was started.
        /// Used for calculating request timing and analyzing execution patterns.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
