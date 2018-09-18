namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionDescribeDescription
    {
        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty(PropertyName = "captions")]
        public IEnumerable<VisionDescribeCaption> Captions { get; set; }
    }
}