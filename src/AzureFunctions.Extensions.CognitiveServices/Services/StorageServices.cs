using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.CognitiveServices.Services
{
    public class StorageServices
    {
        public static async Task<Byte[]> GetFileBytes(string blobPath, string blobConnection)
        {
            if (!CloudStorageAccount.TryParse(blobConnection, out var storageAccount))
            {
                throw new ArgumentException($"The storage account connection you provided is not valid.");
            }

            var path = Path.GetDirectoryName(blobPath);
            var filename = Path.GetFileName(blobPath);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(path);
            var blob = container.GetBlockBlobReference(filename);

            using (MemoryStream memStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memStream);

                return memStream.ToArray();
            }
        }
    }
}