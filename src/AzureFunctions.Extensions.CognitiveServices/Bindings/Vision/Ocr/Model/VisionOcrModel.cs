namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionOcrModel
    {
        [JsonProperty("textAngle")]
        public double TextAngle { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("regions")]
        public IList<Region> Regions { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}