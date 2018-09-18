namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    using System;

    [Flags]
    public enum VisionAnalysisOptions
    {
        All = 0,
        Categories = 1,
        Tags = 2,
        Description = 4,
        Faces = 8,
        ImageType = 16,
        Color = 32,
        Adult = 64,
        Celebrities = 128,
        Landmarks = 256
    }
}