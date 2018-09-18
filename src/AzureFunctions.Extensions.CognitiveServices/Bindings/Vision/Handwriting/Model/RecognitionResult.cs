namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class RecognitionResult
    {
        [JsonProperty("lines")]
        public IList<Line> Lines { get; set; }
    }
}