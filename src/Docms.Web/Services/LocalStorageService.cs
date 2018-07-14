using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class LocalStorageService : IStorageService
    {
        private string _basePath;
        private string _thumbnailPath;

        private readonly FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

        public LocalStorageService()
        {
            _basePath = Path.GetFullPath("App_Data\\Files");
            _thumbnailPath = Path.GetFullPath("App_Data\\Thumbnails");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
            if (!Directory.Exists(_thumbnailPath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public Task<BlobInfo> GetBlobInfo(string blobName)
        {
            var file = FindFile(_basePath, blobName);
            if (file == null)
            {
                return Task.FromResult(default(BlobInfo));
            }
            var fileInfo = new FileInfo(file);
            provider.TryGetContentType(fileInfo.Name, out var contentType);
            return Task.FromResult(new BlobInfo()
            {
                BlobName = blobName,
                ContentType = contentType ?? "application/octet-stream",
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length,
                ETag = "\"" + ETagGenerator.GetETag(blobName, File.ReadAllBytes(fileInfo.FullName)) + "\""
            });
        }

        public async Task<Stream> OpenStreamAsync(string blobName)
        {
            return await Task.FromResult(new FileStream(FindFile(_basePath, blobName), FileMode.Open, FileAccess.Read));
        }

        public async Task<string> UploadFileAsync(Stream stream, string contentType)
        {
            try
            {
                var pro = new FileExtensionContentTypeProvider();
                var ext = pro.Mappings.FirstOrDefault(kv => kv.Value == contentType).Key;
                if (ext == "jpe")
                {
                    ext = "jpg";
                }
                var blobName = Guid.NewGuid().ToString();
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }
                using (var fs = new FileStream(Path.Combine(_basePath, blobName + ext), FileMode.CreateNew, FileAccess.Write))
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

        public async Task DeleteFileAsync(string blobName)
        {
            await Task.Yield();
            var filePath = FindFile(_basePath, blobName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public Task<BlobInfo> GetThumbInfo(string blobName, string size)
        {
            var fileInfo = new FileInfo(string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(blobName), size));
            if (!fileInfo.Exists)
            {
                return Task.FromResult(default(BlobInfo));
            }
            var pro = new FileExtensionContentTypeProvider();
            provider.TryGetContentType(fileInfo.Name, out var contentType);
            return Task.FromResult(new BlobInfo()
            {
                BlobName = fileInfo.Name,
                ContentType = contentType ?? "application/octet-stream",
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length,
                ETag = "\\" + ETagGenerator.GetETag(blobName, File.ReadAllBytes(fileInfo.FullName)) + "\\"
            });
        }

        public async Task<Stream> OpenThumbnailStreamAsync(string blobName, string size = "small")
        {
            var name = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(blobName), size);
            return await Task.FromResult(new FileStream(Path.Combine(_basePath, name), FileMode.Open, FileAccess.Read));
        }

        public string FindFile(string path, string blobName)
        {
            foreach (var item in Directory.GetFiles(path))
            {
                if (Path.GetFileName(item).StartsWith(blobName))
                {
                    return item;
                }
            }
            return null;
        }
    }
    public static class ETagGenerator
    {
        public static string GetETag(string key, byte[] contentBytes)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var combinedBytes = Combine(keyBytes, contentBytes);

            return GenerateETag(combinedBytes);
        }

        private static string GenerateETag(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                string hex = BitConverter.ToString(hash);
                return hex.Replace("-", "");
            }
        }

        private static byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }
}
