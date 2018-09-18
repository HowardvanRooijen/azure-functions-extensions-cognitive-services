namespace AzureFunctions.Extensions.CognitiveServices.Config
{
    public class PollingPolicy
    {
        public int MaxRetryAttempts { get; set; } = 10;

        public int WaitBetweenRetry { get; set; } = 1;

        public int MaxRetryWaitTimeInSeconds { get; set; } = 120;
    }
}