namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    using Newtonsoft.Json;
    using System.IO;

    public class VisionDomainRequest : VisionRequestBase
    {
        public const string CELEBRITY_DOMAIN = "Celebrity";
        public const string LANDMARK_DOMAIN = "Landmark";

        public VisionDomainRequest() { }

        public VisionDomainRequest(Stream image) : base(image) { }

        public VisionDomainRequest(byte[] image) : base(image) { }

        public VisionDomainRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("options")]
        public VisionDomainOptions Domain { get; set; } = VisionDomainOptions.None;
    }
}