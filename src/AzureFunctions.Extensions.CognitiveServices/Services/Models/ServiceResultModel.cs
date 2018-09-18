namespace AzureFunctions.Extensions.CognitiveServices.Services.Models
{
    using System.Net.Http.Headers;

    public class ServiceResultModel
    {
        public int HttpStatusCode { get; set; }

        public string Contents { get; set; }

        public byte[] Binary { get; set; }

        public HttpResponseHeaders Headers { get; set; }
    }
}