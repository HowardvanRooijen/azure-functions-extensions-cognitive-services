namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting.Model
{
    using Newtonsoft.Json;

    public class VisionHandwritingModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("succeeded")]
        public bool Succeeded { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }

        [JsonProperty("recognitionResult")]
        public RecognitionResult RecognitionResult { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}