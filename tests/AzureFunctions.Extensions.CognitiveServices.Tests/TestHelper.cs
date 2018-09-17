﻿namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    #region Using Directives

    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
    using AzureFunctions.Extensions.CognitiveServices.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    #endregion 

    public class TestHelper
    {
        public static async Task ExecuteFunction<FunctionType, BindingType>(ICognitiveServicesClient client, string functionReference)
        {
            IExtensionConfigProvider binding = null;

            if(typeof(BindingType) == typeof(VisionAnalysisBinding))
            {
                binding = new VisionAnalysisBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionDescribeBinding))
            {
                binding = new VisionDescribeBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionHandwritingBinding))
            {
                binding = new VisionHandwritingBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionOcrBinding))
            {
                binding = new VisionOcrBinding(new FakeLoggerFactory());
            }

            if (typeof(BindingType) == typeof(VisionThumbnailBinding))
            {
                binding = new VisionThumbnailBinding(new FakeLoggerFactory());
            }

            (binding as IVisionBinding).Client = client;

            var jobHost = NewHost<FunctionType>(binding);
            
            var args = new Dictionary<string, object>();
            await jobHost.CallAsync(functionReference, args);
        }

        public static JobHost NewHost<T>(IExtensionConfigProvider ext)
        {
            /*
            JobHostConfiguration config = new JobHostConfiguration();
            config.HostId = Guid.NewGuid().ToString("n");
            config.StorageConnectionString = null;
            config.DashboardConnectionString = null;
            config.TypeLocator = new FakeTypeLocator<T>();
            config.AddExtension(ext);
            config.NameResolver = new NameResolver();
            */

            return new JobHost(null, null);
        }
    }

    public class NameResolver : INameResolver
    {
        IConfigurationRoot config;

        public NameResolver()
        {
            this.config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

        }
        public string Resolve(string name)
        {
            name = $"Values:{name}";

            var value = this.config[name].ToString();

            return value;
        }
    }

    public class FakeTypeLocator<T> : ITypeLocator
    {
        public IReadOnlyList<Type> Types => new Type[] { typeof(T) };

        public IReadOnlyList<Type> GetTypes()
        {
            return Types;
        }
    }

    public class FakeLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return default(ILogger);
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }
}