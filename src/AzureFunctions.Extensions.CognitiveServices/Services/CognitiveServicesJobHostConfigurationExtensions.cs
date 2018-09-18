namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
    using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
    using Microsoft.Azure.WebJobs;
    using System;

    public static class CognitiveServicesJobHostConfigurationExtensions
    {
        /// <summary>
        ///     Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to configure.</param>
        /// <returns>Returns the provided <see cref="IWebJobsBuilder" />.</returns>
        public static IWebJobsBuilder AddCognitiveServices(this IWebJobsBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.AddExtension<VisionAnalysisBinding>();
            builder.AddExtension<VisionDescribeBinding>();
            builder.AddExtension<VisionDomainBinding>();
            builder.AddExtension<VisionHandwritingBinding>();
            builder.AddExtension<VisionOcrBinding>();
            builder.AddExtension<VisionThumbnailBinding>();

            // .BindOptions<DurableTaskOptions>();
            //.Services.AddSingleton<IConnectionStringResolver, WebJobsConnectionStringProvider>();

            return builder;
        }

/*        /// <summary>
        ///     Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to configure.</param>
        /// <param name="options">The configuration options for this extension.</param>
        /// <returns>Returns the provided <see cref="IWebJobsBuilder" />.</returns>
        public static IWebJobsBuilder AddCognitiveServices(this IWebJobsBuilder builder, IOptions<DurableTaskOptions> options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (options == null) throw new ArgumentNullException(nameof(options));

            builder.AddCognitiveServices();
            builder.Services.AddSingleton(options);

            return builder;
        }

        /// <summary>
        ///     Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder" /> to configure.</param>
        /// <param name="configure">
        ///     An <see cref="Action{DurableTaskOptions}" /> to configure the provided
        ///     <see cref="DurableTaskOptions" />.
        /// </param>
        /// <returns>Returns the modified <paramref name="builder" /> object.</returns>
        public static IWebJobsBuilder AddCognitiveServices(this IWebJobsBuilder builder, Action<DurableTaskOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.AddCognitiveServices();
            builder.Services.Configure(configure);

            return builder;
        }*/
    }
}