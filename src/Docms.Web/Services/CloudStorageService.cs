using Docms.Web.Config;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class CloudStorageService : IStorageService
    {
        private readonly StorageSettings _storageSettings;
        private readonly CloudStorageAccount _account;

        public CloudStorageService(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings?.Value;
            _account = CloudStorageAccount.Parse(_storageSettings?.ConnectionString);
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileextension)
        {
            try
            {
                var client = _account.CreateCloudBlobClient();
                var container = client.GetContainerReference("files");
                await container.CreateIfNotExistsAsync();

                var blobName = Guid.NewGuid().ToString() + fileextension;
                var blob = container.GetBlockBlobReference(blobName);
                await blob.UploadFromStreamAsync(stream);
                return blobName;
            }
            finally
            {
                stream.Dispose();
            }
        }

        public async Task<Stream> OpenStreamAsync(string blobName)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("files");
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference(blobName);
            return await blob.OpenReadAsync();
        }
    }
}
