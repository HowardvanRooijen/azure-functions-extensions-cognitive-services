namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Region
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("lines")]
        public IList<Line> Lines { get; set; }
    }
}