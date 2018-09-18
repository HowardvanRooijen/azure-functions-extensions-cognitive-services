namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Logging;
    using System;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr.Model;

    #endregion 

    public class VisionOcrBinding : IExtensionConfigProvider, IVisionBinding
    {
        private readonly ILoggerFactory loggerFactory;

        public VisionOcrBinding(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ICognitiveServicesClient Client { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {
            this.LoadClient();

            var visionRule = context.AddBindingRule<VisionOcrAttribute>();

            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<VisionOcrModel>(GetVisionOcrModel);
            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.Url).BindToInput<VisionOcrModel>(GetVisionOcrModel);
            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.Client).BindToInput<VisionOcrClient>(attr => new VisionOcrClient(this, attr, this.loggerFactory));
        }

        private void LoadClient()
        {
            if (this.Client is null)
            {
                this.Client = new CognitiveServicesClient(new RetryPolicy(), this.loggerFactory);
            }
        }

        private VisionOcrModel GetVisionOcrModel(VisionOcrAttribute attribute)
        {
            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionOcrClient(this, attribute, this.loggerFactory);
            var request = new VisionOcrRequest();

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

            var task = client.OCRAsync(request);
            task.Wait();

            return task.Result;
        }
    }
}