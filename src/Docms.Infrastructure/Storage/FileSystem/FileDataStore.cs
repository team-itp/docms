using Docms.Domain.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.FileSystem
{
    public class FileDataStore : IDataStore
    {
        private readonly string _basePath;

        public FileDataStore(string basePath)
        {
            Directory.CreateDirectory(basePath);
            _basePath = basePath;
        }

        public string CreateKey()
        {
            var time = DateTime.UtcNow;
            var guid = Guid.NewGuid();
            return string.Format("{0:yyyyMMddHH}/{1}", time, guid);
        }

        public async Task<IData> CreateAsync(string key, Stream stream)
        {
            var fullpath = Path.Combine(_basePath, key);
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            using (var fs = File.OpenWrite(fullpath))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            return new FileData(key, fullpath);
        }

        public Task<IData> CreateAsync(string key, Stream stream, long sizeOfStream)
        {
            return CreateAsync(key, stream);
        }

        public Task DeleteAsync(string key)
        {
            var fullpath = Path.Combine(_basePath, key);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
            return Task.CompletedTask;
        }

        public Task<IData> FindAsync(string key)
        {
            var fullpath = Path.Combine(_basePath, key);
            if (!File.Exists(fullpath))
            {
                return Task.FromResult(default(IData));
            }
            return Task.FromResult<IData>(new FileData(key, fullpath));
        }

        public IEnumerable<string> ListAllKeys()
        {
            return ListAllFiles(_basePath).Select(p => p.Substring(_basePath.Length));
        }

        private IEnumerable<string> ListAllFiles(string path)
        {
            foreach (var directoryPath in Directory.GetDirectories(path))
            {
                foreach (var filePath in ListAllFiles(directoryPath))
                {
                    yield return filePath;
                }
            }
            foreach (var filePath in Directory.GetFiles(path))
            {
                yield return filePath;
            }
        }
    }
}
