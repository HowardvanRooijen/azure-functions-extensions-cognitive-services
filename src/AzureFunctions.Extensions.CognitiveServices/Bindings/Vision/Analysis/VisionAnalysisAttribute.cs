using Microsoft.Azure.WebJobs.Description;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionAnalysisAttribute : VisionAttributeBase
    {
    }
}