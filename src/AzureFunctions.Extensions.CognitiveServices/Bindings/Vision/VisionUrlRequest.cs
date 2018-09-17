using Newtonsoft.Json;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public class VisionUrlRequest
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}