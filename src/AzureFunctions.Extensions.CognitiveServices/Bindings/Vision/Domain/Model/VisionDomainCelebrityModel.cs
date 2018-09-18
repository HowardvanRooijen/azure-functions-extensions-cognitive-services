namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model
{
    using Newtonsoft.Json;

    public class VisionDomainCelebrityModel
    {
        [JsonProperty("result")]
        public CelebrityResult Result { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public CelebrityMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}