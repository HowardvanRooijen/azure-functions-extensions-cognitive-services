namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services.Models;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Timeout;
    using Polly.Wrap;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    #endregion 

    public class CognitiveServicesClient : ICognitiveServicesClient
    {
        private static HttpClient httpClient = new HttpClient();
        private PolicyWrap<HttpResponseMessage> retryPolicyWrapper;
        private ILogger logger;


        public CognitiveServicesClient(RetryPolicy retryPolicy, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory?.CreateLogger("Host.Bindings.CognitiveServicesClient");

            Random random = new Random();

            var timeoutPolicy = Policy
                .TimeoutAsync(TimeSpan.FromSeconds(retryPolicy.MaxRetryWaitTimeInSeconds), TimeoutStrategy.Pessimistic);

            var throttleRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == (HttpStatusCode)429)
                .WaitAndRetryAsync(retryPolicy.MaxRetryAttemptsAfterThrottle,
                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(random.Next(0, 1000)),
                                   onRetry: (exception, retryCount, context) =>
                                   {
                                       this.logger.LogWarning($"Cognitive Service - Retry {retryCount} of {context.PolicyKey}, due to 429 throttling.");
                                   });

            this.retryPolicyWrapper = timeoutPolicy.WrapAsync(throttleRetryPolicy);
        }

        public HttpClient GetHttpClientInstance()
        {
            return httpClient;
        }

        public async Task<ServiceResultModel> PostAsync(string uri, string key, StringContent content, ReturnType returnType)
        {
            var httpResponse = await this.retryPolicyWrapper.ExecuteAsync(async () => {

                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

                return await httpClient.PostAsync(uri, content);
            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
                result.Headers = httpResponse.Headers;
            }

            if (returnType == ReturnType.Binary)
            {
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
                result.Headers = httpResponse.Headers;
            }

            return result;
        }

        public async Task<ServiceResultModel> PostAsync(string uri, string key, ByteArrayContent content, ReturnType returnType)
        {
            var httpResponse = await this.retryPolicyWrapper.ExecuteAsync(async () => {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

                return await httpClient.PostAsync(uri, content);
            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
                result.Headers = httpResponse.Headers;
            }

            if (returnType == ReturnType.Binary)
            {
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
                result.Headers = httpResponse.Headers;
            }

            return result;
        }

        public async Task<ServiceResultModel> GetAsync(string uri, string key, ReturnType returnType)
        {
            var httpResponse = await this.retryPolicyWrapper.ExecuteAsync(async () => {

                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

                return await httpClient.GetAsync(uri);
            });

            var result = new ServiceResultModel { HttpStatusCode = (int)httpResponse.StatusCode };

            if (returnType == ReturnType.String)
            {
                result.Headers = httpResponse.Headers;
                result.Contents = await httpResponse.Content.ReadAsStringAsync();
            }

            if (returnType == ReturnType.Binary)
            {
                result.Headers = httpResponse.Headers;
                result.Binary = await httpResponse.Content.ReadAsByteArrayAsync();
            }

            return result;
        }
    }
}