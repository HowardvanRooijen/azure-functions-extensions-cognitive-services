namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Services.Models;
    using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    #endregion 

    public class TestCognitiveServicesClient : ICognitiveServicesClient
    {
        public Task<ServiceResultModel> GetAsync(string uri, string key, ReturnType returnType)
        {
            throw new NotImplementedException();
        }

        public HttpClient GetHttpClientInstance()
        {
            return new HttpClient();
        }

        public Task<ServiceResultModel> PostAsync(string uri, string key, StringContent content, ReturnType returnType)
        {
            ServiceResultModel result = null;

            if (uri.Contains("analyze"))
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
            }

            if (uri.Contains("vision"))
            {
                result =  new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
            }

            if (uri.Contains("describe"))
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionDescribeResults };
            }

            if (returnType == ReturnType.Binary)
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Binary = MockResults.SamplePhoto };
            }

            return Task.FromResult<ServiceResultModel>(result);
        }

        public Task<ServiceResultModel> PostAsync(string uri, string key, ByteArrayContent content, ReturnType returnType)
        {
            ServiceResultModel result = null;

            if (returnType == ReturnType.String)
            {
                if (uri.Contains("analyze"))
                {
                    result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
                }

                if (uri.Contains("vision"))
                {
                    result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionAnalysisResults };
                }

                if (uri.Contains("describe"))
                {
                    result = new ServiceResultModel { HttpStatusCode = 200, Contents = MockResults.VisionDescribeResults };
                }
            }

            if (returnType == ReturnType.Binary)
            {
                result = new ServiceResultModel { HttpStatusCode = 200, Binary = MockResults.SamplePhoto };
            }

            return Task.FromResult<ServiceResultModel>(result);
        }
    }
}