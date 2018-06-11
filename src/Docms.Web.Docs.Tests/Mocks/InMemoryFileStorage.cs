using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Docs.Mocks
{
    class InMemoryFileStorage : IFileStorage
    {
        public Dictionary<string, byte[]> Storage { get; } = new Dictionary<string, byte[]>();
        public Dictionary<string, DocumentFileInfo> FileInfoMap { get; } = new Dictionary<string, DocumentFileInfo>();

        public async Task<DocumentFileInfo> GetFileInfoAsync(string path)
        {
            return await Task.FromResult(FileInfoMap[path]);
        }

        public async Task<DocumentFileInfo> SaveAsync(string path, Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var byteArr = ms.ToArray();
            Storage.Add(path, byteArr);
            var now = DateTime.Now;
            string mediaType;
            if (!MediaType.TryGetMediaType(path, out mediaType))
            {
                mediaType = "application/octet-stream";
            }

            var info = new DocumentFileInfo()
            {
                MediaType = mediaType,
                Name = Path.GetFileName(path),
                Path = path,
                Size = byteArr.Length,
                Created = now,
                Modified = now
            };
            FileInfoMap.Add(path, info);
            return await Task.FromResult(info);
        }
    }
}
