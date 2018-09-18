namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VisionColor
    {
        [JsonProperty(PropertyName = "dominantColorForeground")]
        public string DominantColorForeground { get; set; }

        [JsonProperty(PropertyName = "dominantColorBackground")]
        public string DominantColorBackground { get; set; }

        [JsonProperty(PropertyName = "dominantColors")]
        public IEnumerable<string> DominantColors { get; set; }

        [JsonProperty(PropertyName = "accentColor")]
        public string AccentColor { get; set; }

        [JsonProperty(PropertyName = "isBwImg")]
        public bool IsBwImg { get; set; }
    }
}