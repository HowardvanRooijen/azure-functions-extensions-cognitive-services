namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using System;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
    using AzureFunctions.Extensions.CognitiveServices.Config;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    #endregion 

    public class VisionThumbnailTests
    {
        private static byte[] visionThumbnailResult;

        private readonly ITestOutputHelper testOutputHelper;

        public VisionThumbnailTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task TestVisionThumbnailImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => 
                TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithTooBigImageBytes", this.testOutputHelper));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task TestVisionThumbnailMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() => 
                TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailMissingFile", this.testOutputHelper));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);
        }

        [Fact]
        public async Task TestVisionThumbnailWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithImageBytes", this.testOutputHelper);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public async Task TestVisionThumbnailWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithTooBigImageBytesWithResize", this.testOutputHelper);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        [Fact]
        public async Task TestVisionThumbnailWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            await TestHelper.ExecuteFunction<VisionFunctions, VisionThumbnailBinding>(client, "VisionFunctions.VisionThumbnailWithUrl", this.testOutputHelper);

            Assert.Equal(MockResults.SamplePhoto.Length, visionThumbnailResult.Length);
        }

        private class VisionFunctions
        {
            public async Task VisionThumbnailKeyvault([VisionThumbnail(Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest();

                var result = await client.ThumbnailAsync(request);
            }

            public async Task VisionThumbnailMissingFile([VisionThumbnail(Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest();

                var result = await client.ThumbnailAsync(request);
            }

            public async Task VisionThumbnailWithImageBytes([VisionThumbnail(Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest { ImageBytes = MockResults.SamplePhoto };

                var result = await client.ThumbnailAsync(request);

                visionThumbnailResult = result;
            }

            public async Task VisionThumbnailWithTooBigImageBytes([VisionThumbnail(AutoResize = false, Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest { ImageBytes = MockResults.SamplePhotoTooBig };

                var result = await client.ThumbnailAsync(request);
            }

            public async Task VisionThumbnailWithTooBigImageBytesWithResize([VisionThumbnail(AutoResize = true, Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest { ImageBytes = MockResults.SamplePhotoTooBig };

                visionThumbnailResult = await client.ThumbnailAsync(request);
            }

            public async Task VisionThumbnailWithUrl([VisionThumbnail(Width = "100", Height = "100")] VisionThumbnailClient client)
            {
                var request = new VisionThumbnailRequest { ImageUrl = "http://www.blah" };

                visionThumbnailResult = await client.ThumbnailAsync(request);
            }
        }
    }
}