namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    using AzureFunctions.Extensions.CognitiveServices.Services;

    public interface IVisionBinding
    {
        ICognitiveServicesClient Client { get; set; }
    }
}