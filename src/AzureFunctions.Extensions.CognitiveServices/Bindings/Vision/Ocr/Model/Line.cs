namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Line
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("words")]
        public IList<Word> Words { get; set; }
    }
}