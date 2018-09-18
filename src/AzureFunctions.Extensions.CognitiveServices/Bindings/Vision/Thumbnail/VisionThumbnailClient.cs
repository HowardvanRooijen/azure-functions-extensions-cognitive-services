﻿namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Services.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    #endregion 

    public class VisionThumbnailClient
    {
        private readonly IVisionBinding visionBinding;
        private readonly VisionThumbnailAttribute visionThumbnailAttribute;
        private readonly ILogger logger;

        public VisionThumbnailClient(IVisionBinding visionBinding, VisionThumbnailAttribute visionThumbnailAttribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.visionThumbnailAttribute = visionThumbnailAttribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionThumbnail");
        }

        public async Task<byte[]> ThumbnailAsync(VisionThumbnailRequest request)
        {
            Stopwatch stopwatch = null;

            var visionOperation = await this.MergePropertiesAsync(request, this.visionBinding, this.visionThumbnailAttribute);

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
                        var message = string.Format(VisionExceptionMessages.FileTooLargeAfterResize, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
                        this.logger.LogWarning(message);
                        throw new ArgumentException(message);
                    }
                }
            }

            return await this.SubmitRequestAsync(visionOperation);
        }

        private async Task<byte[]> SubmitRequestAsync(VisionThumbnailRequest request)
        {
            Stopwatch sw = new Stopwatch();

            string uri = $"{request.Url}/generateThumbnail?width={request.Width}&height={request.Height}&smartCropping={request.SmartCropping.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace($"Submitting Vision Thumbnail Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.Binary);

                sw.Stop();

                this.logger.LogMetric("VisionRequestDurationMillisecond", sw.ElapsedMilliseconds);
            }
            else
            {
                using (ByteArrayContent content = new ByteArrayContent(request.ImageBytes))
                {
                    requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.Binary);
                }
            }

            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                this.logger.LogTrace($"Thumbnail Request Results");

                return requestResult.Binary;
            }

            VisionErrorModel error = JsonConvert.DeserializeObject<VisionErrorModel>(requestResult.Contents);
            if (requestResult.HttpStatusCode == (int)System.Net.HttpStatusCode.BadRequest)
            {
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

        private async Task<VisionThumbnailRequest> MergePropertiesAsync(VisionThumbnailRequest operation, IVisionBinding config, VisionThumbnailAttribute attr)
        {
            var visionOperation = new VisionThumbnailRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                Height = attr.Height,
                Width = attr.Width,
                SmartCropping = attr.SmartCropping,
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
