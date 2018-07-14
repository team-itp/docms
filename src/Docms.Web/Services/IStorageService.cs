using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public interface IStorageService
    {
        Task<BlobInfo> GetBlobInfo(string blobName);
        Task<Stream> OpenStreamAsync(string blobName);
        Task<string> UploadFileAsync(Stream stream, string contentType);
        Task DeleteFileAsync(string blobName);

        Task<BlobInfo> GetThumbInfo(string blobName, string size);
        Task<Stream> OpenThumbnailStreamAsync(string blobName, string size);
    }

    public sealed class BlobInfo
    {
        public string ContentType { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public long FileSize { get; set; }
        public string ETag { get; set; }
        public string BlobName { get; internal set; }
    }
}
