using Akka.Actor;
using xUnitV3LoadFramework.Core.Messages;
using xUnitV3LoadFramework.Core.Models;

namespace xUnitV3LoadFramework.Core.Actors
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

			Receive<GetLoadResultMessage>(_ =>
			{
				Sender.Tell(new LoadResult
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