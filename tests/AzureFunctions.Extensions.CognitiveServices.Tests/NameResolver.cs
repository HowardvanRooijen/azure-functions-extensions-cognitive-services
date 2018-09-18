namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;

    public class NameResolver : INameResolver
    {
        IConfigurationRoot config;

        public NameResolver()
        {
            this.config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

        }
        public string Resolve(string name)
        {
            name = $"Values:{name}";

            return this.config[name];
        }
    }
}