using AzureFunctions.Extensions.CognitiveServices.Services;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public interface IVisionBinding
    {
        ICognitiveServicesClient Client { get; set; }
    }
}