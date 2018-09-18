namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model
{
    using Newtonsoft.Json;

    public class VisionDescribeMetadata
    {
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "format")]
        public string Format { get; set; }
    }
}