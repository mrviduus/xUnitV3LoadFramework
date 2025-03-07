using Akka.Actor;
using LoadTestRunner.Messages;
using LoadTestRunner.Models;

namespace LoadTestRunner.Actors
{
    public class ResultCollectorActor : ReceiveActor
    {
        private readonly string _scenarioName;
        private int _total;
        private int _success;
        private int _failure;

        public ResultCollectorActor(string scenarioName)
        {
            _scenarioName = scenarioName;

            Receive<StepResultMessage>(msg =>
            {
                _total++;
                if (msg.IsSuccess)
                    _success++;
                else
                    _failure++;
            });

            Receive<GetLoadTestResultMessage>(_ =>
            {
                Sender.Tell(new LoadTestResult
                {
                    ScenarioName = _scenarioName,
                    Total = _total,
                    Success = _success,
                    Failure = _failure
                });
            });
        }
    }
}