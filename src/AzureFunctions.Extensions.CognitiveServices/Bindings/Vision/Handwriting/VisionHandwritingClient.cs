using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    public class VisionHandwritingClient
    {
        private readonly IVisionBinding visionBinding;
        private readonly VisionHandwritingAttribute visionHandwritingAttribute;
        private readonly ILogger logger;

        public VisionHandwritingClient(IVisionBinding visionBinding, VisionHandwritingAttribute visionHandwritingAttribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.visionHandwritingAttribute = visionHandwritingAttribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionHandwriting");
        }

        public async Task<VisionHandwritingModel> HandwritingAsync(VisionHandwritingRequest request)
        {
            Stopwatch imageResizeSw = null;

            var visionOperation = await this.MergePropertiesAsync(request, this.visionBinding, this.visionHandwritingAttribute);

            if (request.IsUrlImageSource == false)
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

                    imageResizeSw = new Stopwatch();
                    imageResizeSw.Start();

                    visionOperation.ImageBytes = ImageResizeService.ResizeImage(visionOperation.ImageBytes);

                    imageResizeSw.Stop();

                    this.logger.LogMetric("VisionOcrImageResizeDurationMillisecond", imageResizeSw.ElapsedMilliseconds);

                    if (visionOperation.Oversized)
                    {
                        var message = string.Format(VisionExceptionMessages.FileTooLargeAfterResize, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                        this.logger.LogWarning(message);
                        throw new ArgumentException(message);
                    }
                }
            }

            return await this.SubmitRequestAsync(visionOperation);
        }

        private async Task<VisionHandwritingModel> SubmitRequestAsync(VisionHandwritingRequest request)
        {
            Stopwatch sw = new Stopwatch();

            //ocr/language=unk&detectOrientation=true
            string uri = $"{request.Url}/recognizeText?handwriting={request.Handwriting.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace($"Submitting Vision Handwriting Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);
                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                this.logger.LogMetric("VisionHandwritingDurationMillisecond", sw.ElapsedMilliseconds);
            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.Accepted)
            {

                var operationLocation = string.Empty;

                operationLocation = requestResult.Headers.GetValues("Operation-Location").FirstOrDefault();

                this.logger.LogTrace($"Handwriting Request Async Operation Url (Polling) : {operationLocation}");

                return await CheckForResult(operationLocation, request);
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.BadRequest)
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

        private async Task<VisionHandwritingModel> CheckForResult(string operationUrl, VisionHandwritingRequest request)
        {
            var policy = new PollingPolicy();

            Random jitter = new Random();

            var timeoutPolicy = Policy
               .TimeoutAsync(TimeSpan.FromSeconds(policy.MaxRetryWaitTimeInSeconds), TimeoutStrategy.Pessimistic);

            var pollingRetryPolicy = Policy
                .HandleResult<VisionHandwritingModel>(r => r.Status != "Succeeded")
                .WaitAndRetryAsync(policy.MaxRetryAttempts,
                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
                                   onRetry: (exception, retryCount, context) =>
                                   {
                                       this.logger.LogWarning($"Cognitive Service - Polling for Handwriting {retryCount} of {context.PolicyKey}, due to no status of Succeeded.");
                                   }
                );

            var pollingWrapper = timeoutPolicy.WrapAsync(pollingRetryPolicy);

            var visionHandwritingModel = await pollingWrapper.ExecuteAsync(async () => {

                var requestResult = await this.visionBinding.Client.GetAsync(operationUrl, request.Key, ReturnType.String);

                if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<VisionHandwritingModel>(requestResult.Contents);

                }

                if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.BadRequest)
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
            });

            return visionHandwritingModel;
        }

        private async Task<VisionHandwritingRequest> MergePropertiesAsync(VisionHandwritingRequest operation, IVisionBinding config, VisionHandwritingAttribute attr)
        {
            var visionOperation = new VisionHandwritingRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
                Handwriting = attr.Handwriting ?? operation.Handwriting
            };

            if (string.IsNullOrEmpty(visionOperation.Key) && string.IsNullOrEmpty(visionOperation.SecureKey))
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