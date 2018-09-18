namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Logging;
    using System;

    #endregion 

    public class VisionAnalysisBinding : IExtensionConfigProvider, IVisionBinding
    {
        public ICognitiveServicesClient Client {get; set;}

        private readonly ILoggerFactory loggerFactory;

        public VisionAnalysisBinding(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            this.LoadClient();

            var visionAnalysisRule = context.AddBindingRule<VisionAnalysisAttribute>();

            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<VisionAnalysisModel>(GetVisionAnalysisModel);
            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.Url).BindToInput<VisionAnalysisModel>(GetVisionAnalysisModel);
            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.Client).BindToInput<VisionAnalysisClient>(attr => new VisionAnalysisClient(this, attr, loggerFactory));
        }

        private void LoadClient()
        {
            if (this.Client is null)
            {
                this.Client = new CognitiveServicesClient(new RetryPolicy(), this.loggerFactory);
            }
        }

        private VisionAnalysisModel GetVisionAnalysisModel(VisionAnalysisAttribute visionAnalysisAttribute)
        {
            if (visionAnalysisAttribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            visionAnalysisAttribute.Validate();

            var client = new VisionAnalysisClient(this, visionAnalysisAttribute, this.loggerFactory);
            var request = new VisionAnalysisRequest();

            if (visionAnalysisAttribute.ImageSource == ImageSource.BlobStorage)
            {
                var fileTask = StorageServices.GetFileBytes(visionAnalysisAttribute.BlobStoragePath, visionAnalysisAttribute.BlobStorageAccount);
                fileTask.Wait();

                request.ImageBytes = fileTask.Result;

            } else
            {
                request.ImageUrl = visionAnalysisAttribute.ImageUrl;
            }

            var task = client.AnalyzeAsync(request);
            result.Wait();

            return result.Result;
        }
    }
}