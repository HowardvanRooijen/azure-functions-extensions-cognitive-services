namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    using Newtonsoft.Json;

    public class VisionUrlRequest
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}