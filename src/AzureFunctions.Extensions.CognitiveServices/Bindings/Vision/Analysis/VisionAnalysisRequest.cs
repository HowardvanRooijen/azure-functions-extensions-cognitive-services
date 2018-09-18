namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    using Newtonsoft.Json;
    using System.IO;

    public class VisionAnalysisRequest : VisionRequestBase
    {
        public VisionAnalysisRequest() { }

        public VisionAnalysisRequest(Stream image) : base(image) { }

        public VisionAnalysisRequest(byte[] image) : base(image) { }

        public VisionAnalysisRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("options")]
        public VisionAnalysisOptions Options { get; set; } = VisionAnalysisOptions.All;
    }
}