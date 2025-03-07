namespace xUnitLoadRunnerLib.Messages
{
    public class StepResultMessage
    {
        public bool IsSuccess { get; }

        public StepResultMessage(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}