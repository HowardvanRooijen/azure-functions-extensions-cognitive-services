namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionAnalysisModel
    {
        [JsonProperty(PropertyName = "categories")]
        public IEnumerable<VisionCategory> Categories { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<VisionTag> Tags { get; set; }

        [JsonProperty(PropertyName = "description")]
        public VisionDescription Description { get; set; }

        [JsonProperty(PropertyName = "faces")]
        public IEnumerable<VisionFace> Faces { get; set; }

        [JsonProperty(PropertyName = "color")]
        public VisionColor Color { get; set; }

        [JsonProperty(PropertyName = "imageType")]
        public VisionImageType ImageType { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public VisionMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}