using Microsoft.Azure.WebJobs.Description;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionDescribeAttribute : VisionAttributeBase
    {
    }
}