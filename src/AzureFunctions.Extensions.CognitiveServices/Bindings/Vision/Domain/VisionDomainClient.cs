namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    #region Using Directives

    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain.Model;
    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Services.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    #endregion 

    public class VisionDomainClient
    {
        private readonly ILogger logger;
        private readonly IVisionBinding visionBinding;
        private readonly VisionDomainAttribute visionDomainAttribute;

        public VisionDomainClient(IVisionBinding visionBinding, VisionDomainAttribute visionDomainAttribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.visionDomainAttribute = visionDomainAttribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionDomain");
        }

        public Task<VisionDomainCelebrityModel> AnalyzeCelebrityAsync(VisionDomainRequest request)
        {
            return this.AnalyzeAsync<VisionDomainCelebrityModel>(request);
        }

        public Task<VisionDomainLandmarkModel> AnalyzeLandmarkAsync(VisionDomainRequest request)
        {
            return this.AnalyzeAsync<VisionDomainLandmarkModel>(request);
        }

        private async Task<T> AnalyzeAsync<T>(VisionDomainRequest request)
        {
            Stopwatch stopwatch = null;

            var visionOperation = await this.MergePropertiesAsync(request, this.visionBinding, this.visionDomainAttribute);

            if (!request.IsUrlImageSource)
            {
                if (visionOperation.ImageBytes == null || visionOperation.ImageBytes.Length == 0)
                {
                    this.logger.LogWarning(VisionExceptionMessages.FileMissing);
                    throw new ArgumentException(VisionExceptionMessages.FileMissing);
                }

                if (ImageResizeService.IsImage(visionOperation.ImageBytes) == false)
                {
                    this.logger.LogWarning(VisionExceptionMessages.InvalidFileType);
                    throw new ArgumentException(VisionExceptionMessages.InvalidFileType);
                }

                if (visionOperation.Oversized && visionOperation.AutoResize == false)
                {
                    var message = string.Format(VisionExceptionMessages.FileTooLarge, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);

                    this.logger.LogWarning(message);

                    throw new ArgumentException(message);
                }

                if (visionOperation.Oversized && visionOperation.AutoResize)
                {
                    this.logger.LogTrace("Resizing Image");

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    visionOperation.ImageBytes = ImageResizeService.ResizeImage(visionOperation.ImageBytes);

                    stopwatch.Stop();

                    this.logger.LogMetric("VisionAnalysisImageResizeDurationMillisecond", stopwatch.ElapsedMilliseconds);

                    if (visionOperation.Oversized)
                    {
                        var message = string.Format(VisionExceptionMessages.FileTooLargeAfterResize, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);

                        this.logger.LogWarning(message);

                        throw new ArgumentException(message);
                    }
                }
            }

            return await this.SubmitRequestAsync<T>(visionOperation);
        }

        private string GetVisionOperationParameters(VisionDomainRequest request)
        {
            var optionsParam = string.Empty;

            switch (request.Domain)
            {
                case VisionDomainOptions.Celebrity:
                    optionsParam = "models/celebrities/analyze";
                    break;

                case VisionDomainOptions.Landmark:
                    optionsParam = "models/landmarks/analyze ";
                    break;
            }

            return optionsParam;
        }

        private async Task<VisionDomainRequest> MergePropertiesAsync(VisionDomainRequest operation, IVisionBinding config, VisionDomainAttribute attr)
        {
            //Attributes do not allow for enum types so we have to validate
            //the string passed into the attribute to ensure it matches
            //a valid VisionDomainOption. 
            var attrDomain = VisionDomainOptions.None;

            if (!string.IsNullOrEmpty(attr.Domain))
            {
                var valid = Enum.TryParse(attr.Domain, out attrDomain);

                if (!valid)
                {
                    var message = string.Format(VisionExceptionMessages.InvalidDomainName, attr.Domain);
                    this.logger.LogWarning(message);

                    throw new ArgumentException(message);
                }
            }
            else
            {
                if (operation.Domain == VisionDomainOptions.None)
                {
                    var message = string.Format(VisionExceptionMessages.InvalidDomainName, "None");
                    this.logger.LogWarning(message);

                    throw new ArgumentException(message);
                }
            }

            var visionOperation = new VisionDomainRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                Domain = attrDomain == VisionDomainOptions.None ? operation.Domain : attrDomain,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes
            };

            if (string.IsNullOrEmpty(visionOperation.Key) && string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                this.logger.LogWarning(VisionExceptionMessages.KeyMissing);
                throw new ArgumentException(VisionExceptionMessages.KeyMissing);
            }

            if (!string.IsNullOrEmpty(visionOperation.SecureKey))
            {
                var httpClient = this.visionBinding.Client.GetHttpClientInstance();

                visionOperation.Key = await KeyVaultServices.GetValue(visionOperation.SecureKey, httpClient);
            }

            return visionOperation;
        }

        private async Task<T> SubmitRequestAsync<T>(VisionDomainRequest request)
        {
            var sw = new Stopwatch();

            var requestParameters = this.GetVisionOperationParameters(request);

            var uri = $"{request.Url}/{requestParameters}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace("Submitting Vision Domain Request");

                var urlRequest = new VisionUrlRequest {Url = request.ImageUrl};
                var requestContent = JsonConvert.SerializeObject(urlRequest);
                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                this.logger.LogMetric("VisionDomainRequestDurationMillisecond", sw.ElapsedMilliseconds);
            }
            else
            {
                using (var content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);
                }
            }

            if (requestResult.HttpStatusCode == (int) HttpStatusCode.OK)
            {
                this.logger.LogTrace($"Analysis Request Results: {requestResult.Contents}");

                return JsonConvert.DeserializeObject<T>(requestResult.Contents);
            }

            if (requestResult.HttpStatusCode == (int) HttpStatusCode.BadRequest)
            {
                var error = JsonConvert.DeserializeObject<VisionErrorModel>(requestResult.Contents);
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
    }
}