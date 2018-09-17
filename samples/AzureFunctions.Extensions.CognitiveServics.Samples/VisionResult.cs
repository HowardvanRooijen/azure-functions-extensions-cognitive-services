using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctions.Extensions.CognitiveServics.Samples
{
    public class VisionResult : TableEntity
    {

        public VisionResult(string id, string partitionKey)
        {
            this.RowKey = id;
            this.PartitionKey = partitionKey;
        }

        public string ResultJson { get; set; }

    }
}
