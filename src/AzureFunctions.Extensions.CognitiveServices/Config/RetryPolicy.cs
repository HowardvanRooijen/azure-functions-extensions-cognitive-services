namespace AzureFunctions.Extensions.CognitiveServices.Config
{
    public class RetryPolicy
    {
        public int MaxRetryAttemptsAfterThrottle { get; set; } = 3;

        public int MaxRetryWaitTimeInSeconds { get; set; } = 90;
    }
}