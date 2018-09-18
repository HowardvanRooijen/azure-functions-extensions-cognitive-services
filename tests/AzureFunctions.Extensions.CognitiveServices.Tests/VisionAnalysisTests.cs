namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using System;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis.Model;
    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;

    #endregion

    public class VisionAnalysisTests
    {
        private static VisionAnalysisModel visionAnalysisImageBytesResizeResult;
        private static VisionAnalysisModel visionAnalysisImageBytesResult;

        private static VisionAnalysisModel visionAnalysisUrlResult;

        [Fact]
        public static async Task TestVisionAnalysisImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithTooBigImageBytes"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);
        }

        [Fact]
        public static async Task TestVisionAnalysisMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisMissingFile"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithImageBytes");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithTooBigImageBytesWithResize");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithUrl");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        private class VisionFunctions
        {
            public async Task VisionAnalysisKeyvault([VisionAnalysis] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);
            }

            public async Task VisionAnalysisMissingFile([VisionAnalysis] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);
            }

            public async Task VisionAnalysisWithImageBytes([VisionAnalysis] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest {ImageBytes = MockResults.SamplePhoto};

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResult = result;
            }

            public async Task VisionAnalysisWithTooBigImageBytes([VisionAnalysis(AutoResize = false)] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();

                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.AnalyzeAsync(request);
            }

            public async Task VisionAnalysisWithTooBigImageBytesWithResize([VisionAnalysis(AutoResize = true)] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResizeResult = result;
            }

            public async Task VisionAnalysisWithUrl([VisionAnalysis] VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest {ImageUrl = "http://www.blah"};

                visionAnalysisUrlResult = await client.AnalyzeAsync(request);
            }
        }
    }
}