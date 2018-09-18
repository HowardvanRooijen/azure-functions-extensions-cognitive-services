namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Logging;
    using System;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model;

    #endregion 

    public class VisionDomainBinding : IExtensionConfigProvider, IVisionBinding
    {
        private readonly ILoggerFactory loggerFactory;

        public VisionDomainBinding(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ICognitiveServicesClient Client { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {
            LoadClient();

            var visionDomainRule = context.AddBindingRule<VisionDomainAttribute>();

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<VisionDomainLandmarkModel>(GetVisionLandmarkModel);
            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Url).BindToInput<VisionDomainLandmarkModel>(GetVisionLandmarkModel);
            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<VisionDomainCelebrityModel>(GetVisionCelebrityModel);
            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Url).BindToInput<VisionDomainCelebrityModel>(GetVisionCelebrityModel);
            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Client).BindToInput<VisionDomainClient>(attr => new VisionDomainClient(this, attr, this.loggerFactory));
        }

        private void LoadClient()
        {
            if (this.Client is null)
            {
                this.Client = new CognitiveServicesClient(new RetryPolicy(), this.loggerFactory);
            }
        }

        private VisionDomainCelebrityModel GetVisionCelebrityModel(VisionDomainAttribute visionDomainAttribute)
        {

            if (visionDomainAttribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            visionDomainAttribute.Validate();

            var client = new VisionDomainClient(this, visionDomainAttribute, this.loggerFactory);
            var request = BuildRequest(visionDomainAttribute);
            var task = client.AnalyzeCelebrityAsync(request);

            task.Wait();

            return task.Result;
        }

        private VisionDomainLandmarkModel GetVisionLandmarkModel(VisionDomainAttribute attribute)
        {
            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionDomainClient(this, attribute, this.loggerFactory);
            var request = BuildRequest(attribute);
            var result = client.AnalyzeLandmarkAsync(request);

            result.Wait();

            return result.Result;
        }

        private VisionDomainRequest BuildRequest(VisionDomainAttribute attribute)
        {
            var request = new VisionDomainRequest();

            if (attribute.ImageSource == ImageSource.BlobStorage)
            {
                var fileTask = StorageServices.GetFileBytes(attribute.BlobStoragePath, attribute.BlobStorageAccount);
                fileTask.Wait();

                request.ImageBytes = fileTask.Result;
            }
            else
            {
                request.ImageUrl = attribute.ImageUrl;
            }

            return request;
        }
    }
}