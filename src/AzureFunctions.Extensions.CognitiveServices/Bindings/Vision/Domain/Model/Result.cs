namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Result
    {
        [JsonProperty("landmarks")]
        public IList<Landmark> Landmarks { get; set; }
    }
}