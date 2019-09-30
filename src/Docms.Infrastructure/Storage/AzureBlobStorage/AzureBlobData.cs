using Docms.Domain.Documents;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.AzureBlobStorage
{
    public class AzureBlobData : IData
    {
        private readonly CloudBlobContainer _container;

        public AzureBlobData(CloudBlobContainer container, string blobName, long length, string hash)
        {
            _container = container;
            StorageKey = blobName;
            Length = length;
            Hash = hash;
        }

        public string StorageKey { get; }

        public long Length { get; }

        public string Hash { get; }

        public Task<Stream> OpenStreamAsync()
        {
            var blob = _container.GetBlobReference(StorageKey);
            return blob.OpenReadAsync();
        }
    }
}
