namespace xUnitV3LoadFramework.Extensions;

/// <summary>
/// Abstract base class implementing the Specification pattern for behavior-driven testing.
/// Provides a structured approach to test organization with setup, execution, and cleanup phases.
/// </summary>
public abstract class Specification
{
    /// <summary>
    /// Executes the primary test action or behavior being verified.
    /// Override this method to implement the specific behavior under test.
    /// </summary>
    protected virtual void Because() { }

    /// <summary>
    /// Performs cleanup operations after test execution.
    /// Override this method to implement resource disposal and state cleanup.
    /// </summary>
    protected virtual void DestroyContext() { }

    /// <summary>
    /// Sets up the test context and preconditions before execution.
    /// Override this method to implement test data setup and initialization.
    /// </summary>
    protected virtual void EstablishContext() { }

    /// <summary>
    /// Internal method called by the framework to perform cleanup after test execution.
    /// Ensures that DestroyContext is called to maintain test isolation.
    /// </summary>
    internal void OnFinish()
    {
        // Execute cleanup operations to maintain test isolation
        DestroyContext();
    }

    /// <summary>
    /// Internal method called by the framework to initialize and execute the test.
    /// Orchestrates the setup and execution phases of the specification pattern.
    /// </summary>
    internal void OnStart()
    {
        // Establish the test context and preconditions
        EstablishContext();
        // Execute the primary behavior being tested
        Because();
    }
}
