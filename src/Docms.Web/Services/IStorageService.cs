using Docms.Web.Config;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream stream, string fileextension);
        Task<Stream> OpenStreamAsync(string blobName);
    }
}
