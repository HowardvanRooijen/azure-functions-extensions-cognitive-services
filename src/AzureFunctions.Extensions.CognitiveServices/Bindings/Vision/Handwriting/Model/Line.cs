namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Line
    {

        [JsonProperty("boundingBox")]
        public IList<int> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("words")]
        public IList<Word> Words { get; set; }
    }
}