using Microsoft.Azure.WebJobs.Description;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionOcrAttribute : VisionAttributeBase
    {
        public bool? DetectOrientation { get; set; } 
    }
}