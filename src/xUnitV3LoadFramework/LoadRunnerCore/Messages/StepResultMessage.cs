namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
	public class StepResultMessage
	{
		public bool IsSuccess { get; }
		public double Latency { get; }
		public double QueueTime { get; }

		public StepResultMessage(bool isSuccess, double latency, double queueTime = 0)
		{
			IsSuccess = isSuccess;
			Latency = latency;
			QueueTime = queueTime;
		}
	}
}