namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using Newtonsoft.Json;

    public class VisionImageType
    {
        [JsonProperty(PropertyName = "clipArtType")]
        public int ClipArtType { get; set; }

        [JsonProperty(PropertyName = "lineDrawingType")]
        public int LineDrawingType { get; set; }
    }
}