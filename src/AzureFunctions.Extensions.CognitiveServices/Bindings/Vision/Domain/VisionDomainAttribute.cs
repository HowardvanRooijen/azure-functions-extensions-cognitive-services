using Microsoft.Azure.WebJobs.Description;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionDomainAttribute : VisionAttributeBase
    {
        /// <summary>
        /// String representation of VisionDomainOptions so we can set
        /// options settings via the attribute which only supports strings
        /// </summary>
        public string Domain { get; set; }
    }
}