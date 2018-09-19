﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Taken from the Durable Tasks Extension - https://github.com/Azure/azure-functions-durable-extension

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    internal class TestLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper testOutput;
        private readonly Func<string, LogLevel, bool> filter;

        public TestLoggerProvider(ITestOutputHelper testOutput, Func<string, LogLevel, bool> filter = null)
        {
            this.testOutput = testOutput;
            this.filter = filter; //// ?? new LogCategoryFilter().Filter;
        }

        public IList<TestLogger> CreatedLoggers { get; } = new List<TestLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new TestLogger(this.testOutput, categoryName, this.filter);
            this.CreatedLoggers.Add(logger);

            return logger;
        }

        public IEnumerable<LogMessage> GetAllLogMessages()
        {
            return this.CreatedLoggers.SelectMany(l => l.LogMessages);
        }

        public void Dispose()
        {
        }
    }
}