namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class CelebrityResult
    {
        [JsonProperty("celebrities")]
        public IList<Celebrity> Celebrities { get; set; }
    }
}