namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using System;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe.Model;
    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    #endregion

    public class VisionDescribeTests
    {
        private static VisionDescribeModel visionDescribeImageBytesResizeResult;
        private static VisionDescribeModel visionDescribeImageBytesResult;
        private static VisionDescribeModel visionDescribeUrlResult;

        private readonly ITestOutputHelper testOutputHelper;

        public VisionDescribeTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task TestVisionAnalysisWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithUrl", this.testOutputHelper);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task TestVisionDescribeImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() =>
                TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithTooBigImageBytes", this.testOutputHelper));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task TestVisionDescribeMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() =>
                TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeMissingFile", this.testOutputHelper));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);
        }

        [Fact]
        public async Task TestVisionDescribeWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithImageBytes", this.testOutputHelper);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task TestVisionDescribeWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithTooBigImageBytesWithResize", this.testOutputHelper);

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        private class VisionFunctions
        {
            public async Task VisionDescribeKeyvault([VisionDescribe] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);
            }

            public async Task VisionDescribeMissingFile([VisionDescribe] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);
            }

            public async Task VisionDescribeWithImageBytes([VisionDescribe] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();

                visionDescribeImageBytesResult = await client.DescribeAsync(request);
            }

            public async Task VisionDescribeWithTooBigImageBytes([VisionDescribe(AutoResize = false)] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest {AutoResize = false, ImageBytes = MockResults.SamplePhotoTooBig};

                var result = await client.DescribeAsync(request);
            }

            public async Task VisionDescribeWithTooBigImageBytesWithResize([VisionDescribe] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest { ImageBytes = MockResults.SamplePhotoTooBig };

                visionDescribeImageBytesResizeResult = await client.DescribeAsync(request);
            }

            public async Task VisionDescribeWithUrl([VisionDescribe] VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();
                request.ImageUrl = "http://www.blah";

                visionDescribeUrlResult = await client.DescribeAsync(request);
            }
        }
    }
}