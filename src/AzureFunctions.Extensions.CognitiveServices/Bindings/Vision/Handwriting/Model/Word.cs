namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Word
    {

        [JsonProperty("boundingBox")]
        public IList<int> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}