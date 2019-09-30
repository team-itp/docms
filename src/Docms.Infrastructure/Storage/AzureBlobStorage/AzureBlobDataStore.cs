using Docms.Domain.Documents;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            return await UploadData(key, stream, container).ConfigureAwait(false);
        }

        private static async Task<IData> UploadData(string key, Stream stream, CloudBlobContainer container)
        {
            var blobRef = container.GetBlockBlobReference(key);
            using (var cloudStream = await blobRef.OpenWriteAsync().ConfigureAwait(false))
            {
                var buf = new byte[8192];
                var readBytes = default(int);
                var length = 0;
                using (var sha1 = SHA1.Create())
                {
                    while (true)
                    {
                        readBytes = await stream.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                        length += readBytes;
                        if (readBytes <= 0)
                        {
                            await cloudStream.CommitAsync();
                            break;
                        }
                        sha1.TransformBlock(buf, 0, readBytes, null, 0);
                        await cloudStream.WriteAsync(buf, 0, readBytes).ConfigureAwait(false);
                    }
                    sha1.TransformFinalBlock(buf, 0, 0);
                    var hashBytes = sha1.Hash;

                    var hash = BitConverter.ToString(hashBytes).Replace("-", "");
                    blobRef.Metadata.Add(HASH_KEY, hash);
                    await blobRef.SetMetadataAsync().ConfigureAwait(false);
                    return new AzureBlobData(container, key, length, hash);
                }
            }
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

        public IEnumerable<string> ListAllKeys()
        {
            var container = _client.GetContainerReference(_baseContainerName);
            if (!container.ExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                yield break;
            }
            var token = new BlobContinuationToken();
            do
            {
                var segment = container.ListBlobsSegmentedAsync(token).ConfigureAwait(false).GetAwaiter().GetResult();
                var entries = segment.Results.ToList();
                foreach (var d in entries.OfType<CloudBlobDirectory>())
                {
                    foreach (var b in ListAllBlobs(d))
                    {
                        yield return b;
                    }
                }
                foreach (var b in entries.OfType<CloudBlob>())
                {
                    yield return b.Name;
                }
                token = segment.ContinuationToken;
            } while (token != null);
        }

        private IEnumerable<string> ListAllBlobs(CloudBlobDirectory blobDirectory)
        {
            var token = new BlobContinuationToken();
            do
            {
                var segment = blobDirectory.ListBlobsSegmentedAsync(token).ConfigureAwait(false).GetAwaiter().GetResult();
                var entries = segment.Results.ToList();
                foreach (var d in entries.OfType<CloudBlobDirectory>())
                {
                    foreach (var b in ListAllBlobs(d))
                    {
                        yield return b;
                    }
                }
                foreach (var b in entries.OfType<CloudBlob>())
                {
                    yield return b.Name;
                }
                token = segment.ContinuationToken;
            } while (token != null);
        }
    }
}
