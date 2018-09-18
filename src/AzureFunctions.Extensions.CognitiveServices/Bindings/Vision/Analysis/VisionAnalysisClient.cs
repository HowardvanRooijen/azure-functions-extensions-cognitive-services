namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Services.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model;

    #endregion

    public class VisionAnalysisClient
    {
        private readonly IVisionBinding visionBinding;
        private readonly VisionAnalysisAttribute visionAnalysisAttribute;
        private readonly ILogger logger;

        public VisionAnalysisClient(IVisionBinding visionBinding, VisionAnalysisAttribute visionAnalysisAttribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.visionAnalysisAttribute = visionAnalysisAttribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionAnalysis");
        }

        public async Task<VisionAnalysisModel> AnalyzeAsync(VisionAnalysisRequest request)
        {
            Stopwatch stopwatch = null;

            var visionOperation = await MergeProperties(request, this.visionBinding, this.visionAnalysisAttribute);

            if (!request.IsUrlImageSource)
            {
        
                if (visionOperation.ImageBytes == null || visionOperation.ImageBytes.Length == 0)
                {
                    this.logger.LogWarning(VisionExceptionMessages.FileMissing);
                    throw new ArgumentException(VisionExceptionMessages.FileMissing);
                }

                if (!ImageResizeService.IsImage(visionOperation.ImageBytes))
                {
                    this.logger.LogWarning(VisionExceptionMessages.InvalidFileType); 
                    throw new ArgumentException(VisionExceptionMessages.InvalidFileType);
                }

                if (visionOperation.Oversized && !visionOperation.AutoResize)
                {
                    var message = string.Format(VisionExceptionMessages.FileTooLarge,
                                                    VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                    this.logger.LogWarning(message);
                    throw new ArgumentException(message);
                }
                else if (visionOperation.Oversized && visionOperation.AutoResize)
                {
                    this.logger.LogTrace("Resizing Image");

                    stopwatch = new Stopwatch();

                    stopwatch.Start();

                    visionOperation.ImageBytes = ImageResizeService.ResizeImage(visionOperation.ImageBytes);

                    stopwatch.Stop();

                    this.logger.LogMetric("VisionAnalysisImageResizeDurationMillisecond", stopwatch.ElapsedMilliseconds);

                    if (visionOperation.Oversized)
                    {
                        var message = string.Format(VisionExceptionMessages.FileTooLargeAfterResize, 
                                                        VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                        this.logger.LogWarning(message);
                        throw new ArgumentException(message);
                    }
                }
            }

            return await this.SubmitRequestAsync(visionOperation);
        }

        private async Task<VisionAnalysisModel> SubmitRequestAsync(VisionAnalysisRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string requestParameters = GetVisionOperationParameters(request);

            string uri = $"{request.Url}/analyze?{requestParameters}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace($"Submitting Vision Analysis Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                this.logger.LogMetric("VisionRequestDurationMillisecond", sw.ElapsedMilliseconds);
            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                this.logger.LogTrace($"Analysis Request Results: {requestResult.Contents}");

                return JsonConvert.DeserializeObject<VisionAnalysisModel>(requestResult.Contents);
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.BadRequest)
            {

                VisionErrorModel error = JsonConvert.DeserializeObject<VisionErrorModel>(requestResult.Contents);
                var message = string.Format(VisionExceptionMessages.CognitiveServicesException, error.Code, error.Message);

                this.logger.LogWarning(message);

                throw new Exception(message);
            }
            else
            {
                var message = string.Format(VisionExceptionMessages.CognitiveServicesException, requestResult.HttpStatusCode, requestResult.Contents);

                this.logger.LogError(message);

                throw new Exception(message);
            }
        }

        private string GetVisionOperationParameters(VisionAnalysisRequest request)
        {
            VisionAnalysisOptions options = request.Options;

            string optionsParam = string.Empty;
            string visualFeaturesParam = string.Empty;
            string detailsParam = string.Empty;
            string languageParam = "&language=en";

            if (options == VisionAnalysisOptions.All)
            {
                options = VisionAnalysisOptions.Categories |
                                  VisionAnalysisOptions.Celebrities |
                                  VisionAnalysisOptions.Color |
                                  VisionAnalysisOptions.Description |
                                  VisionAnalysisOptions.Faces |
                                  VisionAnalysisOptions.ImageType |
                                  VisionAnalysisOptions.Landmarks |
                                  VisionAnalysisOptions.Tags;
            }

            //Details Parameters
            if (options.HasFlag(VisionAnalysisOptions.Celebrities)
                || options.HasFlag(VisionAnalysisOptions.Landmarks))
            {
                List<string> details = new List<string>();

                if (options.HasFlag(VisionAnalysisOptions.Celebrities))
                {
                    details.Add("Celebrities");
                }

                if (options.HasFlag(VisionAnalysisOptions.Landmarks))
                {
                    details.Add("Landmarks");
                }

                detailsParam = $"&details={string.Join(",", details)}";

                //Remove the Details Flags from the options so they are not
                //included in subsequent operations
                options = options & ~(VisionAnalysisOptions.Celebrities | VisionAnalysisOptions.Landmarks);

            }

            //Visual Features Parameter
            visualFeaturesParam = options.ToString();
            visualFeaturesParam = visualFeaturesParam.Replace(" ", string.Empty);
            visualFeaturesParam = $"visualFeatures={visualFeaturesParam}";

            //Combine All parameter
            optionsParam = $"{visualFeaturesParam}{detailsParam}{languageParam}";

            return optionsParam;
        }

        private async Task<VisionAnalysisRequest> MergeProperties(VisionAnalysisRequest operation, IVisionBinding config, VisionAnalysisAttribute attr)
        {
            var visionOperation = new VisionAnalysisRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? operation.SecureKey,
                AutoResize = attr.AutoResize,
                Options = operation.Options,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
            };

            if(string.IsNullOrEmpty(visionOperation.Key) && string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                this.logger.LogWarning(VisionExceptionMessages.KeyMissing);
                throw new ArgumentException(VisionExceptionMessages.KeyMissing);
            }

            if (!string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                HttpClient httpClient = this.visionBinding.Client.GetHttpClientInstance();

                visionOperation.Key = await KeyVaultServices.GetValue(visionOperation.SecureKey, httpClient);
            }

            return visionOperation;
        }
    }
}