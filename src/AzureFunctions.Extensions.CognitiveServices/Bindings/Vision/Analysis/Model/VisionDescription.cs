namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionDescription
    {
        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty(PropertyName = "captions")]
        public IEnumerable<VisionCaption> Captions { get; set; }
    }
}