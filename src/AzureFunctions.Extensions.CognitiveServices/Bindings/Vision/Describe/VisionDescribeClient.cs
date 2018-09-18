using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public class VisionDescribeClient
    {
        IVisionBinding visionBinding;
        VisionDescribeAttribute attribute;
        ILogger logger;

        public VisionDescribeClient(IVisionBinding visionBinding, VisionDescribeAttribute attribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.attribute = attribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionDescribe");
        }

        public async Task<VisionDescribeModel> DescribeAsync(VisionDescribeRequest request)
        {
            try
            {
                Stopwatch imageResizeSW = null;

                var visionOperation = await MergeProperties(request, this.visionBinding, this.attribute);

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
                        var message = string.Format(VisionExceptionMessages.FileTooLarge, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                        this.logger.LogWarning(message);
                        throw new ArgumentException(message);
                    }

                    if (visionOperation.Oversized && visionOperation.AutoResize)
                    {
                        this.logger.LogTrace("Resizing Image");

                        imageResizeSW = new Stopwatch();

                        imageResizeSW.Start();

                        visionOperation.ImageBytes = ImageResizeService.ResizeImage(visionOperation.ImageBytes);

                        imageResizeSW.Stop();

                        this.logger.LogMetric("VisionAnalysisImageResizeDurationMillisecond", imageResizeSW.ElapsedMilliseconds);

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
            catch(Exception ex )
            {
                throw ex;
            }
        }

        private async Task<VisionDescribeModel> SubmitRequestAsync(VisionDescribeRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string uri = $"{request.Url}/describe?maxCandidates={request.MaxCandidates}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace($"Submitting Vision Describe Request");

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
                this.logger.LogTrace($"Describe Request Results: {requestResult.Contents}");

                return JsonConvert.DeserializeObject<VisionDescribeModel>(requestResult.Contents);
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

        private async Task<VisionDescribeRequest> MergeProperties(VisionDescribeRequest operation, IVisionBinding config, VisionDescribeAttribute attr)
        {
            var visionOperation = new VisionDescribeRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                MaxCandidates = operation.MaxCandidates,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
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