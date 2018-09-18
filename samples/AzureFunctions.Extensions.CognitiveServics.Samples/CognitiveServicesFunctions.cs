namespace AzureFunctions.Extensions.CognitiveServics.Samples
{
    #region Using Directives

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
    using AzureFunctions.Extensions.CognitiveServices.Bindings;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    #endregion 

    [StorageAccount("StorageAccount")]
    public static class CognitiveServicesFunctions
    {
        /// <summary>
        /// Sample calling Vision Analysis Triggered from a blob storage 
        ///     Trigger: Blob Storage
        ///     Vision Binding:  Model Binding w/ Blob Data Source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="model"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("VisionAnalysisModelBlobFunction")]
        public static void VisionAnalysisModelBlobFunctionRun(
           [BlobTrigger("analysismodel/{name}")] Stream stream,
           [VisionAnalysis(BlobStorageAccount = "StorageAccount", BlobStoragePath = "analysismodel/{name}", ImageSource = ImageSource.BlobStorage)] VisionAnalysisModel model,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{model}");
        }

        [FunctionName("VisionAnalysisBlobFunction")]
        public static async Task VisionAnalysisRun(
           [BlobTrigger("analysis/{name}")]Stream stream,
           [Table("VisionResults")]IAsyncCollector<VisionResult> results,
           [VisionAnalysis] VisionAnalysisClient client,
           string name,
           TraceWriter log)
        {
            var result = await client.AnalyzeAsync(new VisionAnalysisRequest(stream));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionAnalysis") { ResultJson = result.ToString() });

            log.Info($"Analysis Results:{result.ToString()}");
        }

        [FunctionName("VisionDesribeBlobFunction")]
        public static async Task VisionDescribeBlobFunction(
            [BlobTrigger("describe/{name}")] Stream stream,
            [Table("VisionResults")] IAsyncCollector<VisionResult> results,
            [VisionDescribe] VisionDescribeClient client,
            string name,
            TraceWriter log)
        {
            var result = await client.DescribeAsync(new VisionDescribeRequest(stream));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDescribe") { ResultJson = result.ToString() });

            log.Info($"Describe Results:{result}");
        }

        [FunctionName("VisionDescribeModelBlobFunction")]
        public static void VisionDescribeModelBlobFunction(
            [BlobTrigger("describe/{name}")]Stream stream,
            [Table("VisionResults")]IAsyncCollector<VisionResult> results,
            [VisionDescribe(BlobStorageAccount = "StorageAccount", BlobStoragePath = "describemodel/{name}", ImageSource = ImageSource.BlobStorage)]VisionDescribeModel result,
            string name,
            TraceWriter log)
        {
            log.Info($"Describe Results:{result.ToString()}");
        }

        [FunctionName("VisionThumbnailBlobFunction")]
        public static async Task VisionThumbnailBlobFunction(
         [BlobTrigger("thumbnail/{name}")]Stream inputStream, 
         [VisionThumbnail(AutoResize = true, Height ="100", Width = "100", SmartCropping =true)] VisionThumbnailClient client,
         [Blob("thumbnailresults/{name}", FileAccess.Write)] Stream outputStream,
         string name,
         TraceWriter log)
        {
            var result = await client.ThumbnailAsync(new VisionThumbnailRequest(inputStream));

            using (MemoryStream stream = new MemoryStream(result))
            {
                await stream.CopyToAsync(outputStream);
            }

            log.Info($"Image thumbnail generated");
        }

        [FunctionName("VisionOcrBlobFunction")]
        public static async Task VisionOcrBlobFunction(
            [BlobTrigger("ocrrequest/{name}")] Stream stream,
            [Table("VisionResults")] IAsyncCollector<VisionResult> results,
            [VisionOcr] VisionOcrClient client,
            string name,
            TraceWriter log)
        {
            var result = await client.OCRAsync(new VisionOcrRequest(stream));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionOCR") { ResultJson = result.ToString() });

            log.Info($"OCR Results:{result.ToString()}");
        }

        [FunctionName("VisionHandwritingBlobFunction")]
        public static async Task VisionHandwritingBlobFunction(
            [BlobTrigger("handwriting/{name}")] Stream stream,
            [Table("VisionResults")] IAsyncCollector<VisionResult> results,
            [VisionHandwriting] VisionHandwritingClient client,
            string name,
            TraceWriter log)
        {
            var result = await client.HandwritingAsync(new VisionHandwritingRequest(stream));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionHandwriting") { ResultJson = result.ToString() });

            log.Info($"Handwriting Results:{result}");
        }

        [FunctionName("VisionCelebrityBlobFunction")]
        public static async Task VisionCelebrityBlobFunction(
           [BlobTrigger("celebrity/{name}")] Stream stream,
           [Table("VisionResults")] IAsyncCollector<VisionResult> results,
           [VisionDomain(Domain = VisionDomainRequest.CELEBRITY_DOMAIN)] VisionDomainClient client,
           string name,
           TraceWriter log)
        {
            var request = new VisionDomainRequest(stream) { Domain = VisionDomainOptions.Celebrity };

            var celebrityResult = await client.AnalyzeCelebrityAsync(request);

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDomain") { ResultJson = celebrityResult.ToString() });

            log.Info($"Celebrity Domain results:{celebrityResult}");
        }

        [FunctionName("VisionLandmarkBlobFunction")]
        public static async Task VisionLandmarkBlobFunction(
          [BlobTrigger("landmarks/{name}")] Stream storageBlob,
          [Table("VisionResults")] IAsyncCollector<VisionResult> results,
          [VisionDomain(Domain = VisionDomainRequest.LANDMARK_DOMAIN)] VisionDomainClient visionclient,
          string name,
          TraceWriter log)
        {
            var landmarkResult = await visionclient.AnalyzeLandmarkAsync(new VisionDomainRequest(storageBlob));

            await results.AddAsync(new VisionResult(Guid.NewGuid().ToString(), "VisionDomain") { ResultJson = landmarkResult.ToString() });

            log.Info($"Celebrity Domain results:{landmarkResult}");
        }
    }
}