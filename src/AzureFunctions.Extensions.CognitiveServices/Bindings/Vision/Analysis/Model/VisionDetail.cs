namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionDetail
    {
        [JsonProperty(PropertyName = "landmarks")]
        public IEnumerable<VisionLandmark> Landmarks { get; set; }

        [JsonProperty(PropertyName = "celebrities")]
        public IEnumerable<VisionCelebrity> Celebrities { get; set; }
    }
}