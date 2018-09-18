namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using Newtonsoft.Json;

    public class VisionCelebrity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }

        [JsonProperty(PropertyName = "faceRectangle")]
        public VisionFaceRectangle FaceRectangle { get; set; }
    }
}