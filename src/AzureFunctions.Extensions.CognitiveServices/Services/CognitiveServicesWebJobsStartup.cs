[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(AzureFunctions.Extensions.CognitiveServices.Services.CognitiveServicesWebJobsStartup))]
namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;

    internal class CognitiveServicesWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddCognitiveServices();
        }
    }
}