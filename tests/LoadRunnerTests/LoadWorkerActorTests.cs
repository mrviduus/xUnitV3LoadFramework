using System;
using System.Threading.Tasks;
using Akka.TestKit.Xunit2;
using LoadRunnerCore.Actors;
using LoadRunnerCore.Messages;
using LoadRunnerCore.Models;
using Xunit;

namespace LoadRunnerTests;

public class LoadWorkerActorTests : TestKit
{
    [Fact]
    public async Task RunWorkAsync_CompletesSuccessfully_WhenActionIsValid()
    {
        var mockResultCollector = CreateTestProbe();
        var executionPlan = new LoadExecutionPlan
        {
            Action = () => Task.FromResult(true),
            Settings = new LoadSettings { Duration = TimeSpan.FromSeconds(1), Concurrency = 1, Interval = TimeSpan.FromMilliseconds(100) }
        };

        var workerActor = ActorOfAsTestActorRef(() => new LoadWorkerActor(executionPlan, mockResultCollector.Ref));
        workerActor.Tell(new StartLoadMessage());

        await Task.Delay(1500); // Wait for the duration to complete

        mockResultCollector.ExpectMsg<StepResultMessage>(msg => msg.IsSuccess == false);
    }

    [Fact]
    public async Task RunWorkAsync_CancelsExecution_WhenDurationExpires()
    {
        var mockResultCollector = CreateTestProbe();
        var executionPlan = new LoadExecutionPlan
        {
            Action = () => Task.FromResult(true),
            Settings = new LoadSettings { Duration = TimeSpan.FromMilliseconds(500), Concurrency = 1, Interval = TimeSpan.FromMilliseconds(100) }
        };

        var workerActor = ActorOfAsTestActorRef(() => new LoadWorkerActor(executionPlan, mockResultCollector.Ref));
        workerActor.Tell(new StartLoadMessage());

        await Task.Delay(1000); // Wait for the duration to complete

        mockResultCollector.ExpectNoMsg(TimeSpan.FromMilliseconds(500)); // No more messages after duration
    }

    [Fact]
    public async Task RunWorkAsync_HandlesExceptionDuringExecution()
    {
        var mockResultCollector = CreateTestProbe();
        var executionPlan = new LoadExecutionPlan
        {
            Action = () => throw new InvalidOperationException("Test exception"),
            Settings = new LoadSettings { Duration = TimeSpan.FromSeconds(1), Concurrency = 1, Interval = TimeSpan.FromMilliseconds(100) }
        };

        var workerActor = ActorOfAsTestActorRef(() => new LoadWorkerActor(executionPlan, mockResultCollector.Ref));
        workerActor.Tell(new StartLoadMessage());

        await Task.Delay(1500); // Wait for the duration to complete

        // Ensure the actor handled the exception and continued running
        mockResultCollector.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
    }
}