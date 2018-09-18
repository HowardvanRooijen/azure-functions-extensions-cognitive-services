namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using Newtonsoft.Json;

    public class VisionFace
    {
        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "faceRectangle")]
        public VisionFaceRectangle FaceRectangle { get; set; }
    }
}