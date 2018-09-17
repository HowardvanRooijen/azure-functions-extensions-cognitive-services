using Newtonsoft.Json;
using System.IO;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public class VisionDescribeRequest : VisionRequestBase
    {
        public VisionDescribeRequest() { }

        public VisionDescribeRequest(Stream image) : base(image) { }

        public VisionDescribeRequest(byte[] image) : base(image) { }

        public VisionDescribeRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("maxCandidates")]
        public int MaxCandidates { get; set; } = 1;
    }
}