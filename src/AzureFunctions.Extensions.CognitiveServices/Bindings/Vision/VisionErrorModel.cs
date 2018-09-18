namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    using Newtonsoft.Json;

    public class VisionErrorModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}