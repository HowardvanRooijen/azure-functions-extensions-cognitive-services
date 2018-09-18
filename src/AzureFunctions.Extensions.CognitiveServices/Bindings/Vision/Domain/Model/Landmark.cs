namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model
{
    using Newtonsoft.Json;

    public class Landmark
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}