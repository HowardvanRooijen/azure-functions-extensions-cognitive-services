namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model
{
    using Newtonsoft.Json;

    public class VisionDescribeCaption
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }
    }
}