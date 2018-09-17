using Newtonsoft.Json;
using System.IO;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public class VisionOcrRequest : VisionRequestBase
    {
        public VisionOcrRequest() { }

        public VisionOcrRequest(Stream image) : base(image) { }

        public VisionOcrRequest(byte[] image) : base(image) { }

        public VisionOcrRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("detectOrientation")]
        public bool DetectOrientation { get; set; } = false;
    }
}