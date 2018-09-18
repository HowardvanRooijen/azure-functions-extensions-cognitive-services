namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Logging;
    using System;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting.Model;

    #endregion 

    public class VisionHandwritingBinding : IExtensionConfigProvider, IVisionBinding
    {
        private readonly ILoggerFactory loggerFactory;

        public VisionHandwritingBinding(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ICognitiveServicesClient Client { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {
            this.LoadClient();

            var visionRule = context.AddBindingRule<VisionHandwritingAttribute>();

            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<VisionHandwritingModel>(GetVisionHandwritingModel);
            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.Url).BindToInput<VisionHandwritingModel>(GetVisionHandwritingModel);
            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.Client).BindToInput<VisionHandwritingClient>(attr => new VisionHandwritingClient(this, attr, this.loggerFactory));
        }

        private void LoadClient()
        {
            if (this.Client is null)
            {
                this.Client = new CognitiveServicesClient(new RetryPolicy(), this.loggerFactory);
            }
        }

        private VisionHandwritingModel GetVisionHandwritingModel(VisionHandwritingAttribute attribute)
        {
            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionHandwritingClient(this, attribute, this.loggerFactory);
            var request = new VisionHandwritingRequest();

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

            var task = client.HandwritingAsync(request);
            task.Wait();

            return task.Result;
        }
    }
}