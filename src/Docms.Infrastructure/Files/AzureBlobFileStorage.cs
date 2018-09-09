using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public class AzureBlobFileStorage : IFileStorage
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _client;
        private readonly string _baseContainerName;

        public AzureBlobFileStorage(string connectionString, string baseContainerName)
        {
            _account = CloudStorageAccount.Parse(connectionString);
            _client = _account.CreateCloudBlobClient();
            _baseContainerName = baseContainerName;
        }

        public Task<Entry> GetEntryAsync(string path)
        {
            return GetEntryAsync(new FilePath(path));
        }

        public async Task<Entry> GetEntryAsync(FilePath path)
        {
            if (path.DirectoryPath != null)
            {
                var blockBlob = await GetBlockBlobAsync(path).ConfigureAwait(false);
                if (await blockBlob.ExistsAsync())
                {
                    return new File(path, this);
                }
            }

            var blobDir = await GetBlobDirectoryAsync(path);
            try
            {
                var result = await blobDir.ListBlobsSegmentedAsync(null);
                if (result.Results.Any())
                {
                    return new Directory(path, this);
                }
            }
            catch (StorageException)
            {
            }
            return default(Entry);
        }

        public Task<Directory> GetDirectoryAsync(string path)
        {
            return GetDirectoryAsync(new FilePath(path));
        }

        public async Task<Directory> GetDirectoryAsync(FilePath path)
        {
            if (path.DirectoryPath != null)
            {
                var blockBlob = await GetBlockBlobAsync(path).ConfigureAwait(false);
                if (await blockBlob.ExistsAsync())
                {
                    throw new InvalidOperationException();
                }
            }
            return new Directory(path, this);
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync(Directory dir)
        {
            var blobDirectory = await GetBlobDirectoryAsync(dir.Path).ConfigureAwait(false);
            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await blobDirectory.ListBlobsSegmentedAsync(continuationToken).ConfigureAwait(false);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);
            return results.Select(e =>
            {
                if (e is CloudBlockBlob)
                {
                    return new File(PathFromUri(e.Uri), this) as Entry;
                }
                if (e is CloudBlobDirectory)
                {
                    return new Directory(PathFromUri(e.Uri), this) as Entry;
                }
                return default(Entry);
            });
        }

        public async Task<Stream> OpenAsync(File file)
        {
            var blockBlob = await GetBlockBlobAsync(file.Path).ConfigureAwait(false);
            return await blockBlob.OpenReadAsync().ConfigureAwait(false);
        }

        public async Task<File> SaveAsync(Directory dir, string filename, string contentType, Stream stream)
        {
            var filePath = dir.Path.Combine(filename);
            var blockBlob = await GetBlockBlobAsync(filePath).ConfigureAwait(false);
            await blockBlob.UploadFromStreamAsync(stream).ConfigureAwait(false);
            blockBlob.Properties.ContentType = contentType;
            await blockBlob.SetPropertiesAsync().ConfigureAwait(false);
            return new File(filePath, this);
        }

        public async Task MoveAsync(FilePath originalPath, FilePath destinationPath)
        {
            if (!await ExistsAsync(originalPath) || await ExistsAsync(destinationPath))
            {
                throw new InvalidOperationException();
            }
            var origBlob = await GetBlockBlobAsync(originalPath).ConfigureAwait(false);
            var destBlob = await GetBlockBlobAsync(destinationPath).ConfigureAwait(false);
            using (var ms = new MemoryStream())
            {
                await origBlob.DownloadToStreamAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                await destBlob.UploadFromStreamAsync(ms);
                destBlob.Properties.ContentType = origBlob.Properties.ContentType;
                await destBlob.SetPropertiesAsync();
                await origBlob.DeleteAsync();
            }
        }

        public async Task DeleteAsync(Entry entry)
        {
            if (entry.Path.DirectoryPath != null)
            {
                var blockBlob = await GetBlockBlobAsync(entry.Path).ConfigureAwait(false);
                if (await blockBlob.DeleteIfExistsAsync())
                {
                    return;
                }
            }

            var blobDir = await GetBlobDirectoryAsync(entry.Path).ConfigureAwait(false);
            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await blobDir.ListBlobsSegmentedAsync(continuationToken).ConfigureAwait(false);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);
            foreach (var item in results)
            {
                if (item is CloudBlockBlob block)
                {
                    await block.DeleteAsync();
                }
            }
        }

        private FilePath PathFromUri(Uri uri)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            var relativeUri = new Uri(container.Uri.ToString() + "/", UriKind.Absolute).MakeRelativeUri(uri);
            return new FilePath(relativeUri.ToString());
        }

        private async Task<CloudBlockBlob> GetBlockBlobAsync(FilePath path)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            await container.CreateIfNotExistsAsync();
            return container.GetBlockBlobReference(path.ToString());
        }

        private async Task<CloudBlobDirectory> GetBlobDirectoryAsync(FilePath path)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            await container.CreateIfNotExistsAsync();
            return container.GetDirectoryReference(path.ToString());
        }

        private async Task<bool> ExistsAsync(FilePath path)
        {
            var entry = await GetEntryAsync(path);
            return entry != null;
        }
    }
}
