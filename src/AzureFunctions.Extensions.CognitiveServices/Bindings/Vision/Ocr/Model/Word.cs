namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr.Model
{
    using Newtonsoft.Json;

    public class Word
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}