namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model
{
    using Newtonsoft.Json;

    public class VisionDescribeModel
    {
        [JsonProperty(PropertyName = "description")]
        public VisionDescribeDescription Description { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public VisionDescribeMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}