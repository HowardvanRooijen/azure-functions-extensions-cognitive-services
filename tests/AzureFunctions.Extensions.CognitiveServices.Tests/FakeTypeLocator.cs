namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.WebJobs;

    public class FakeTypeLocator<T> : ITypeLocator
    {
        public IReadOnlyList<Type> Types => new Type[] { typeof(T) };

        public IReadOnlyList<Type> GetTypes()
        {
            return this.Types;
        }
    }
}