namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using Newtonsoft.Json;

    public class VisionFaceRectangle
    {
        [JsonProperty(PropertyName = "top")]
        public int Top { get; set; }

        [JsonProperty(PropertyName = "left")]
        public int Left { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "length")]
        public int Length { get; set; }
    }
}