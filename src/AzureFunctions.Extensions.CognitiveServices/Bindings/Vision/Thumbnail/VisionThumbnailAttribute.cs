using Microsoft.Azure.WebJobs.Description;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionThumbnailAttribute : VisionAttributeBase
    {
        [AutoResolve()]
        public string Width { get; set; }

        [AutoResolve()]
        public string Height { get; set; }

        public bool SmartCropping { get; set; }
    }
}