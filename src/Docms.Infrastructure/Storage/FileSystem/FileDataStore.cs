using Docms.Domain.Documents;
using System;
using System.IO;
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
            return string.Format("{0}/{1}", time.Ticks % TimeSpan.TicksPerMillisecond, guid);
        }

        public async Task<IData> CreateAsync(string key, Stream stream)
        {
            var fullpath = Path.Combine(_basePath, key);
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            using (var fs = File.OpenWrite(fullpath))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            return new FileData(fullpath);
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
            return Task.FromResult<IData>(new FileData(fullpath));
        }
    }
}
