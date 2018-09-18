using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public class VisionOcrClient
    {
        private readonly IVisionBinding visionBinding;
        private readonly VisionOcrAttribute visionOcrAttribute;
        private readonly ILogger logger;

        public VisionOcrClient(IVisionBinding visionBinding, VisionOcrAttribute visionOcrAttribute, ILoggerFactory loggerFactory)
        {
            this.visionBinding = visionBinding;
            this.visionOcrAttribute = visionOcrAttribute;
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.VisionOcr");
        }

        public async Task<VisionOcrModel> OCRAsync(VisionOcrRequest request)
        {
            Stopwatch imageResizeSw = null;

            var visionOperation = await this.MergePropertiesAsync(request, this.visionBinding, this.visionOcrAttribute);

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
                    var message = string.Format(VisionExceptionMessages.FileTooLarge, VisionConfiguration.MaximumFileSize, visionOperation.ImageBytes.Length);
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

        private async Task<VisionOcrModel> SubmitRequestAsync(VisionOcrRequest request)
        {
            Stopwatch sw = new Stopwatch();

            //ocr/language=unk&detectOrientation=true
            string uri = $"{request.Url}/ocr?detectOrientation={request.DetectOrientation.ToString()}";

            ServiceResultModel requestResult = null;

            if (request.IsUrlImageSource)
            {
                this.logger.LogTrace($"Submitting Vision Ocr Request");

                var urlRequest = new VisionUrlRequest { Url = request.ImageUrl };
                var requestContent = JsonConvert.SerializeObject(urlRequest);

                var content = new StringContent(requestContent);

                sw.Start();

                requestResult = await this.visionBinding.Client.PostAsync(uri, request.Key, content, ReturnType.String);

                sw.Stop();

                this.logger.LogMetric("VisionOCRDurationMillisecond", sw.ElapsedMilliseconds);

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
                this.logger.LogTrace($"OCR Request Results: {requestResult.Contents}");

                return JsonConvert.DeserializeObject<VisionOcrModel>(requestResult.Contents);
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

        private async Task<VisionOcrRequest> MergePropertiesAsync(VisionOcrRequest operation, IVisionBinding config, VisionOcrAttribute attr)
        {
            var visionOperation = new VisionOcrRequest
            {
                Url = attr.VisionUrl ?? operation.Url,
                Key = attr.VisionKey ?? operation.Key,
                SecureKey = attr.SecureKey ?? attr.SecureKey,
                AutoResize = attr.AutoResize,
                ImageUrl = string.IsNullOrEmpty(operation.ImageUrl) ? attr.ImageUrl : operation.ImageUrl,
                ImageBytes = operation.ImageBytes,
                DetectOrientation = attr.DetectOrientation ?? operation.DetectOrientation
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