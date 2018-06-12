using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public class LocalFileStorage : IFileStorage
    {
        private string _basePath;

        public LocalFileStorage(string basePath)
        {
            _basePath = Path.GetFullPath(basePath);
        }

        public async Task<DocumentFileInfo> SaveAsync(string path, Stream stream)
        {
            var pathToSave = Path.Combine(_basePath, path);
            var parentPath = Path.GetDirectoryName(pathToSave);
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            using (var fs = new FileStream(pathToSave, FileMode.CreateNew, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }

            return await GetFileInfoAsync(path);
        }

        public async Task<DocumentFileInfo> GetFileInfoAsync(string path)
        {
            var fileInfo = new FileInfo(Path.Combine(_basePath, path));
            string mediaType;
            if (!MediaType.TryGetMediaType(fileInfo.FullName, out mediaType))
            {
                mediaType = "application/octet-stream";
            }

            var info = new DocumentFileInfo()
            {
                MediaType = mediaType,
                Name = fileInfo.Name,
                Path = ConvertPath(fileInfo.FullName),
                Size = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime,
                ModifiedAt = fileInfo.LastWriteTime
            };
            return await Task.FromResult(info);
        }

        private string ConvertPath(string path)
        {
            var uri = new Uri(Path.GetFullPath(_basePath) + "/", UriKind.Absolute);
            return uri.MakeRelativeUri(new Uri(Path.Combine(_basePath, path), UriKind.Absolute)).ToString();
        }
    }
}
