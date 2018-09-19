﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Taken from the Durable Tasks Extension - https://github.com/Azure/azure-functions-durable-extension

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    internal class LogMessage
    {
        public LogLevel Level { get; set; }

        public EventId EventId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> State { get; set; }

        public Exception Exception { get; set; }

        public string FormattedMessage { get; set; }

        public string Category { get; set; }

        public override string ToString()
        {
            return this.FormattedMessage;
        }
    }
}