using System.Collections.Concurrent;
using System.Diagnostics;
using Akka.Actor;
using xUnitV3LoadFramework.LoadRunnerCore.Actors;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadTests
{
    public class LoadWorkerActorTests : IDisposable
    {
        private readonly ActorSystem _system = ActorSystem.Create("TestSystem");
        private readonly ConcurrentBag<object> _receivedMessages = new();

        public void Dispose()
        {
            _system.Terminate().Wait();
        }

        private class TestResultCollector : ReceiveActor
        {
            private int _successCount;
            private int _failureCount;

            public TestResultCollector(ConcurrentBag<object> messages)
            {
                var messages1 = messages;
                var scenarioName = "TestScenario";
                var latencies = new List<double>();
                var timer = Stopwatch.StartNew();

                Receive<StartLoadMessage>(msg =>
                {
                    messages1.Add(msg);
                });

                Receive<StepResultMessage>(msg =>
                {
                    messages1.Add(msg);
                    if (msg.IsSuccess) _successCount++;
                    else _failureCount++;
                    latencies.Add(msg.Latency);
                });

                Receive<GetLoadResultMessage>(_ =>
                {
                    timer.Stop();
                    var result = new LoadResult
                    {
                        ScenarioName = scenarioName,
                        Total = _successCount + _failureCount,
                        Success = _successCount,
                        Failure = _failureCount,
                        Time = timer.Elapsed.TotalSeconds,
                        MaxLatency = latencies.Any() ? latencies.Max() : 0,
                        MinLatency = latencies.Any() ? latencies.Min() : 0,
                        AverageLatency = latencies.Any() ? latencies.Average() : 0,
                        Percentile95Latency = latencies.Any() ? CalculatePercentile(latencies, 95) : 0,
                        RequestsStarted = _successCount + _failureCount, // For backward compatibility
                        RequestsInFlight = 0
                    };
                    Sender.Tell(result);
                });
            }

            private static double CalculatePercentile(List<double> latencies, double percentile)
            {
                var orderedLatencies = latencies.OrderBy(x => x).ToList();
                var index = (int)Math.Ceiling((percentile / 100) * orderedLatencies.Count) - 1;
                return orderedLatencies[Math.Max(0, index)];
            }
        }

        [Fact]
        public async Task LoadWorkerActor_ShouldNotifyResultCollectorOnStart()
        {
            // Arrange
            var resultCollector = _system.ActorOf(Props.Create(() => new TestResultCollector(_receivedMessages)));
            var executionPlan = CreateTestExecutionPlan();
            var loadWorkerActor = _system.ActorOf(Props.Create(() => new LoadWorkerActor(executionPlan, resultCollector)));

            // Act
            loadWorkerActor.Tell(new StartLoadMessage());

            // Wait for the actor to process the message
            await Task.Delay(100, TestContext.Current.CancellationToken);

            // Assert
            Assert.Contains(_receivedMessages, msg => msg is StartLoadMessage);
        }

        [Fact]
        public async Task LoadWorkerActor_ShouldSendStepResultMessagesToResultCollector()
        {
            // Arrange
            var resultCollector = _system.ActorOf(Props.Create(() => new TestResultCollector(_receivedMessages)));
            var executionPlan = CreateTestExecutionPlan();
            var loadWorkerActor = _system.ActorOf(Props.Create(() => new LoadWorkerActor(executionPlan, resultCollector)));

            // Act
            loadWorkerActor.Tell(new StartLoadMessage());

            // Wait for the actor to process messages
            await Task.Delay(1200, TestContext.Current.CancellationToken);

            // Assert
            Assert.Contains(_receivedMessages, msg => msg is StepResultMessage);
        }

        [Fact]
        public async Task LoadWorkerActor_ShouldSendFinalLoadResultToSender()
        {
            // Arrange
            var resultCollector = _system.ActorOf(Props.Create(() => new TestResultCollector(_receivedMessages)));
            var executionPlan = CreateTestExecutionPlan();
            var loadWorkerActor = _system.ActorOf(Props.Create(() => new LoadWorkerActor(executionPlan, resultCollector)));

            // Act
            var result = await loadWorkerActor.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LoadResult>(result);
            Assert.NotNull(result.ScenarioName);
            Assert.True(result.Time > 0);
        }

        private static LoadExecutionPlan CreateTestExecutionPlan() => new()
        {
            Name = "TestPlan",
            Settings = new LoadSettings
            {
                Duration = TimeSpan.FromSeconds(1),
                Concurrency = 1,
                Interval = TimeSpan.FromMilliseconds(100)
            },
            Action = () => Task.FromResult(true)
        };
    }
}