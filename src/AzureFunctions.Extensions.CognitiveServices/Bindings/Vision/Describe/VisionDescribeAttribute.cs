namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    using Microsoft.Azure.WebJobs.Description;
    using System;

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionDescribeAttribute : VisionAttributeBase
    {
    }
}