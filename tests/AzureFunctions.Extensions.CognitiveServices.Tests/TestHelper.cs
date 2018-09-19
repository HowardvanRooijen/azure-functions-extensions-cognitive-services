namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host.Config;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    #endregion 

    public class TestHelper
    {
        public static async Task ExecuteFunction<FunctionType, BindingType>(ICognitiveServicesClient client, string functionReference, ITestOutputHelper testOutputHelper)
        {
            IExtensionConfigProvider binding = null;

            if(typeof(BindingType) == typeof(VisionAnalysisBinding))
            {
                binding = new VisionAnalysisBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionDescribeBinding))
            {
                binding = new VisionDescribeBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionHandwritingBinding))
            {
                binding = new VisionHandwritingBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionOcrBinding))
            {
                binding = new VisionOcrBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionThumbnailBinding))
            {
                binding = new VisionThumbnailBinding(new FakeLoggerFactory());
            }

            (binding as IVisionBinding).Client = client;

            var jobHost = NewHost<FunctionType>(testOutputHelper);
            
            var args = new Dictionary<string, object>();
            await jobHost.CallAsync(functionReference, args);
        }

        public static JobHost NewHost<T>(ITestOutputHelper testOutputHelper)
        {
            IHost host = new HostBuilder()
                .ConfigureLogging(
                    loggingBuilder =>
                    {
                        loggingBuilder.AddProvider(new TestLoggerProvider(testOutputHelper));
                    })
                .ConfigureWebJobs(
                    webJobsBuilder =>
                    {
                        webJobsBuilder.AddCognitiveServices();
                    })
                .ConfigureServices(
                    serviceCollection =>
                    {
                        ITypeLocator typeLocator = new FakeTypeLocator<T>();
                        serviceCollection.AddSingleton(typeLocator);
                        serviceCollection.AddSingleton(new NameResolver());
                    })
                .Build();

            return (JobHost)host.Services.GetService<IJobHost>();
        }
    }
}