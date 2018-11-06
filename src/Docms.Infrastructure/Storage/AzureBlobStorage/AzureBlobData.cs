using Docms.Domain.Documents;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.AzureBlobStorage
{
    public class AzureBlobData : IData
    {
        private CloudBlobContainer _container;
        private readonly string _blobName;

        public AzureBlobData(CloudBlobContainer container, string blobName, long length, string hash)
        {
            _container = container;
            _blobName = blobName;
            Length = length;
            Hash = hash;
        }

        public long Length { get; }

        public string Hash { get; }

        public Task<Stream> OpenStreamAsync()
        {
            var blob = _container.GetBlobReference(_blobName);
            return blob.OpenReadAsync();
        }
    }
}
