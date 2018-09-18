namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    using Microsoft.Azure.WebJobs.Description;
    using System;

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]

    public class VisionHandwritingAttribute : VisionAttributeBase
    {

        public bool? Handwriting { get; set; } = true;
    }
}