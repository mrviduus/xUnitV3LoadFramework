using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using LoadTestRunner.Actors;
using LoadTestRunner.Messages;
using LoadTestRunner.Models;
using Xunit;

public class LoadTestWorkerActorTests : TestKit
{
    [Fact]
    public async Task RunStepsAsync_CompletesSuccessfully()
    {
        var plan = new LoadTestPlan
        {
            Steps = new[]
            {
                new LoadTestStep { Action = () => Task.FromResult(true) },
                new LoadTestStep { Action = () => Task.FromResult(false) }
            },
            Settings = new LoadExecutionSettings
            {
                Duration = TimeSpan.FromSeconds(2),
                Interval = TimeSpan.FromSeconds(1)
            }
        };

        var resultCollector = CreateTestProbe();
        var worker = ActorOfAsTestActorRef<LoadTestWorkerActor>(Props.Create(() => new LoadTestWorkerActor(plan, resultCollector)));

        worker.Tell(new StartLoadTestMessage());

        var result = await ExpectMsgAsync<LoadTestResult>(TimeSpan.FromSeconds(3));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RunStepsAsync_RespectsCancellation()
    {
        var plan = new LoadTestPlan
        {
            Steps = new[]
            {
                new LoadTestStep { Action = () => Task.FromResult(true) }
            },
            Settings = new LoadExecutionSettings
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Interval = TimeSpan.FromMilliseconds(100)
            }
        };

        var resultCollector = CreateTestProbe();
        var worker = ActorOfAsTestActorRef<LoadTestWorkerActor>(Props.Create(() => new LoadTestWorkerActor(plan, resultCollector)));

        worker.Tell(new StartLoadTestMessage());

        var result = await ExpectMsgAsync<LoadTestResult>(TimeSpan.FromSeconds(1));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RunStepsAsync_HandlesExceptionsGracefully()
    {
        var plan = new LoadTestPlan
        {
            Steps = new[]
            {
                new LoadTestStep { Action = () => throw new InvalidOperationException() }
            },
            Settings = new LoadExecutionSettings
            {
                Duration = TimeSpan.FromSeconds(2),
                Interval = TimeSpan.FromSeconds(1)
            }
        };

        var resultCollector = CreateTestProbe();
        var worker = ActorOfAsTestActorRef<LoadTestWorkerActor>(Props.Create(() => new LoadTestWorkerActor(plan, resultCollector)));

        worker.Tell(new StartLoadTestMessage());

        var result = await ExpectMsgAsync<LoadTestResult>(TimeSpan.FromSeconds(3));
        Assert.NotNull(result);
    }
}