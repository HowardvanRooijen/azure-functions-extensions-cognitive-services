namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    using Newtonsoft.Json;
    using System.IO;
    
    public class VisionHandwritingRequest : VisionRequestBase
    {
        public VisionHandwritingRequest() { }

        public VisionHandwritingRequest(Stream image) : base(image) { }

        public VisionHandwritingRequest(byte[] image) : base(image) { }

        public VisionHandwritingRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("handwriting")]
        public bool Handwriting { get; set; } = true;
    }
}