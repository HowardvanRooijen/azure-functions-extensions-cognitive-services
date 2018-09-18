namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    using Microsoft.Azure.WebJobs.Description;
    using System;

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionAnalysisAttribute : VisionAttributeBase
    {
    }
}