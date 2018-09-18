namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model
{
    using Newtonsoft.Json;

    public class FaceRectangle
    {
        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("left")]
        public int Left { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}