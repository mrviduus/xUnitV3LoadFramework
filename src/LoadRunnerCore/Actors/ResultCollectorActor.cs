using Akka.Actor;
using Akka.Event;
using LoadRunnerCore.Messages;
using LoadRunnerCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace LoadRunnerCore.Actors
{
    public class ResultCollectorActor : ReceiveActor
    {
        private readonly string _scenarioName;
        private int _total;
        private int _success;
        private int _failure;
        private readonly List<double> _latencies = new();
        private readonly ILoggingAdapter _logger = Context.GetLogger();

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

                _latencies.Add(msg.Latency);

                _logger.Debug("Received step result. Success: {0}, Latency: {1:F2} ms", msg.IsSuccess, msg.Latency);
            });

            Receive<GetLoadResultMessage>(_ =>
            {
                var result = new LoadResult
                {
                    ScenarioName = _scenarioName,
                    Total = _total,
                    Success = _success,
                    Failure = _failure,
                    MaxLatency = _latencies.Any() ? _latencies.Max() : 0,
                    MinLatency = _latencies.Any() ? _latencies.Min() : 0,
                    AverageLatency = _latencies.Any() ? _latencies.Average() : 0,
                    Percentile95Latency = _latencies.Any() ? CalculatePercentile(_latencies, 95) : 0
                };

                _logger.Info("Scenario '{0}' completed. {1}", _scenarioName,
                    $"Total: {result.Total}, Success: {result.Success}, Failure: {result.Failure}, " +
                    $"Max Latency: {result.MaxLatency:F2} ms, Min Latency: {result.MinLatency:F2} ms, " +
                    $"Avg Latency: {result.AverageLatency:F2} ms, 95th Percentile: {result.Percentile95Latency:F2} ms");
                
                Sender.Tell(result);
            });
        }

        private static double CalculatePercentile(List<double> latencies, double percentile)
        {
            latencies.Sort();
            var index = (int)System.Math.Ceiling((percentile / 100.0) * latencies.Count) - 1;
            return latencies[System.Math.Min(index, latencies.Count - 1)];
        }
    }
}
