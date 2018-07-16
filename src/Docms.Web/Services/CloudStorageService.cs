using Docms.Web.Config;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
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

        public async Task<string> UploadFileAsync(Stream stream, string contentType)
        {
            try
            {
                var client = _account.CreateCloudBlobClient();
                var container = client.GetContainerReference("files");
                await container.CreateIfNotExistsAsync();

                var blobName = Guid.NewGuid().ToString();
                var blob = container.GetBlockBlobReference(blobName);
                await blob.UploadFromStreamAsync(stream);
                blob.Properties.ContentType = contentType;
                await blob.SetPropertiesAsync();

                var queue = _account.CreateCloudQueueClient();
                var queueRef = queue.GetQueueReference("create-thumbnail");
                await queueRef.CreateIfNotExistsAsync();
                await queueRef.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(new { BlobName = blobName, ContentType = contentType })));

                return blobName;
            }
            finally
            {
                stream.Dispose();
            }
        }

        public async Task<BlobInfo> GetBlobInfo(string blobName)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("files");
            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blob = container.GetBlockBlobReference(blobName);
            if (!await blob.ExistsAsync())
            {
                return null;
            }
            await blob.FetchAttributesAsync();
            return new BlobInfo()
            {
                BlobName = blobName,
                ContentType = blob.Properties.ContentType,
                LastModified = blob.Properties.LastModified,
                FileSize = blob.Properties.Length,
                ETag = blob.Properties.ETag
            };
        }

        public async Task<Stream> OpenStreamAsync(string blobName)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("files");
            var blob = container.GetBlockBlobReference(blobName);
            return await blob.OpenReadAsync();
        }

        public async Task DeleteFileAsync(string blobName)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("files");
            var blob = container.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();

            var thumbContainer = client.GetContainerReference("thumbnails");
            var largeThumb = thumbContainer.GetBlockBlobReference(string.Format("{0}_{1}.png", blobName, "large"));
            await largeThumb.DeleteIfExistsAsync();
            var smallThumb = thumbContainer.GetBlockBlobReference(string.Format("{0}_{1}.png", blobName, "small"));
            await smallThumb.DeleteIfExistsAsync();
        }

        public async Task<BlobInfo> GetThumbInfo(string blobName, string size)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("thumbnails");
            if (!await container.ExistsAsync())
            {
                return null;
            }

            var blob = container.GetBlockBlobReference(string.Format("{0}_{1}.png", blobName, size));
            if (!await blob.ExistsAsync())
            {
                return null;
            }
            await blob.FetchAttributesAsync();
            return new BlobInfo()
            {
                BlobName = blobName,
                ContentType = blob.Properties.ContentType,
                LastModified = blob.Properties.LastModified,
                FileSize = blob.Properties.Length,
                ETag = blob.Properties.ETag
            };
        }

        public async Task<Stream> OpenThumbnailStreamAsync(string blobName, string size)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference("thumbnails");
            var blob = container.GetBlockBlobReference(string.Format("{0}_{1}.png", blobName, size));
            return await blob.OpenReadAsync();
        }
    }
}
