namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using Newtonsoft.Json;

    public class VisionCategory
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public VisionDetail Detail { get; set; }
    }
}