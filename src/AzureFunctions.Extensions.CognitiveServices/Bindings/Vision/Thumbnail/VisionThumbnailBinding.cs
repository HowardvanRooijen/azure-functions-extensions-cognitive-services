using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    public class VisionThumbnailBinding : IExtensionConfigProvider, IVisionBinding
    {
        private ILoggerFactory loggerFactory;

        public VisionThumbnailBinding(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public ICognitiveServicesClient Client { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {
            this.LoadClient();

            var visionRule = context.AddBindingRule<VisionThumbnailAttribute>();

            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.BlobStorage).BindToInput<byte[]>(GetVisionDescribeModel);
            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.Url).BindToInput<byte[]>(GetVisionDescribeModel);
            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.Client).BindToInput<VisionThumbnailClient>(attr => new VisionThumbnailClient(this, attr, this.loggerFactory));
        }

        private void LoadClient()
        {
            if (this.Client is null)
            {
                this.Client = new CognitiveServicesClient(new RetryPolicy(), this.loggerFactory);
            }
        }

        private byte[] GetVisionDescribeModel(VisionThumbnailAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionThumbnailClient(this, attribute, this.loggerFactory);

            var request = new VisionThumbnailRequest();

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

            var result = client.ThumbnailAsync(request);
            result.Wait();

            return result.Result;
        }
    }
}