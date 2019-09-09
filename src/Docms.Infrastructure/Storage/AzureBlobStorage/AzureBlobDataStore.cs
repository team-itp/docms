using Docms.Domain.Documents;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.AzureBlobStorage
{
    public class AzureBlobDataStore : IDataStore
    {
        private const string HASH_KEY = "Hash";
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _client;
        private readonly string _baseContainerName;

        public AzureBlobDataStore(string connectionString, string baseContainerName)
        {
            _account = CloudStorageAccount.Parse(connectionString);
            _client = _account.CreateCloudBlobClient();
            _baseContainerName = baseContainerName;
        }

        public string CreateKey()
        {
            var time = DateTime.UtcNow;
            var guid = Guid.NewGuid();
            return string.Format("{0:yyyy}/{0:MM}/{0:dd}/{0:HH}/{1}", time, guid);
        }

        public Task<IData> CreateAsync(string key, Stream stream)
        {
            return CreateAsync(key, stream, -1);
        }

        public async Task<IData> CreateAsync(string key, Stream stream, long sizeOfStream)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            if (stream.CanSeek)
            {
                var length = stream.Length;
                var hash = Hash.CalculateHash(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return await UploadData(key, stream, container, length, hash).ConfigureAwait(false);
            }
            else if (sizeOfStream > -1 && sizeOfStream < 33_554_432)
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var length = stream.Length;
                var hash = Hash.CalculateHash(ms.ToArray());
                return await UploadData(key, ms, container, length, hash).ConfigureAwait(false);
            }
            else
            {
                var tempfilename = Path.GetTempFileName();
                try
                {
                    using (var fs = File.OpenWrite(tempfilename))
                    {
                        await stream.CopyToAsync(fs);
                    }
                    var fi = new FileInfo(tempfilename);
                    var length = fi.Length;
                    var hash = default(string);
                    using (var fs = File.OpenRead(tempfilename))
                    {
                        hash = Hash.CalculateHash(fs);
                    }
                    using (var fs = File.OpenRead(tempfilename))
                    {
                        return await UploadData(key, fs, container, length, hash).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (File.Exists(tempfilename))
                    {
                        File.Delete(tempfilename);
                    }
                }
            }
        }

        private static async Task<IData> UploadData(string key, Stream stream, CloudBlobContainer container, long length, string hash)
        {
            var blobRef = container.GetBlockBlobReference(key);
            await blobRef.UploadFromStreamAsync(stream).ConfigureAwait(false);
            blobRef.Metadata.Add(HASH_KEY, hash);
            await blobRef.SetMetadataAsync().ConfigureAwait(false);
            return new AzureBlobData(container, key, length, hash);
        }

        public async Task DeleteAsync(string key)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            if (!await container.ExistsAsync().ConfigureAwait(false))
            {
                return;
            }
            var blob = container.GetBlobReference(key);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<IData> FindAsync(string key)
        {
            var container = _client.GetContainerReference(_baseContainerName);
            if (!await container.ExistsAsync().ConfigureAwait(false))
            {
                return null;
            }
            var blob = container.GetBlockBlobReference(key);
            await blob.FetchAttributesAsync().ConfigureAwait(false);
            return new AzureBlobData(container,
                key,
                blob.Properties.Length,
                blob.Metadata.TryGetValue(HASH_KEY, out var value) ? value : null);
        }

        public async Task<IEnumerable<string>> ListAllKeys()
        {
            var container = _client.GetContainerReference(_baseContainerName);
            if (!await container.ExistsAsync().ConfigureAwait(false))
            {
                return null;
            }
            var token = new BlobContinuationToken();
            var list = new List<string>();
            do
            {
                var segment = await container.ListBlobsSegmentedAsync(token).ConfigureAwait(false);
                list.AddRange(segment.Results.Select(d => d.Uri.ToString()));
                token = segment.ContinuationToken;
            } while (string.IsNullOrEmpty(token.NextMarker));
            return list;
        }
    }
}
