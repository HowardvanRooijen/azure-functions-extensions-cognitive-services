namespace AzureFunctions.Extensions.CognitiveServices.Bindings
{
    /// <summary>
    /// Determines the source of the image being analyzed. Each Image Source
    /// has varied required properties.
    /// </summary>
    public enum ImageSource
    {
        /// <summary>
        /// Image source is a publicly accessible url
        /// </summary>
        Url,
        /// <summary>
        /// Image source is from Blob Storage
        /// </summary>
        BlobStorage,
        /// <summary>
        /// Image source is specified within the client binding
        /// </summary>
        Client
    }
}