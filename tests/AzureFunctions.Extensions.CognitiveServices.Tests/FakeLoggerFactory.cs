namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    using System;
    using Microsoft.Extensions.Logging;

    public class FakeLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return default(ILogger);
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }
}