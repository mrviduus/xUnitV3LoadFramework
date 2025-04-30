namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
	public class StepResultMessage
	{
		public bool IsSuccess { get; }
		public double Latency { get; }

		public StepResultMessage(bool isSuccess, double latency)
		{
			IsSuccess = isSuccess;
			Latency = latency;
		}
	}
}