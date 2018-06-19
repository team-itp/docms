using Docms.Web.Config;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class LocalStorageService : IStorageService
    {
        private string _basePath;
        public LocalStorageService()
        {
            _basePath = Path.GetFullPath("Files");
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileextension)
        {
            try
            {
                var blobName = Guid.NewGuid() + fileextension;
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }
                using (var fs = new FileStream(Path.Combine(_basePath, blobName), FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
                return blobName;
            }
            finally
            {
                stream.Dispose();
            }
        }

        public async Task<Stream> OpenStreamAsync(string blobName)
        {
            var stream = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(_basePath, blobName), FileMode.Open, FileAccess.Read))
            {
                await fs.CopyToAsync(stream);
            }
            return stream;
        }
    }
}
