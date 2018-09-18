namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    using Microsoft.Azure.WebJobs.Description;
    using System;

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionOcrAttribute : VisionAttributeBase
    {
        public bool? DetectOrientation { get; set; }
    }
}