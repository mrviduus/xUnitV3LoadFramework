// Define namespace for actor communication messages and coordination contracts
// Contains all message types used for inter-actor communication during load testing
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    /// <summary>
    /// Message sent to load worker actors to initiate load test execution.
    /// Triggers the start of the load testing process with configured parameters.
    /// This message begins the timing clock and activates all worker coordination.
    /// Should be sent only once per test execution to avoid duplicate initialization.
    /// Acts as the primary coordination signal for distributed load test orchestration.
    /// </summary>
    public class StartLoadMessage { }
}