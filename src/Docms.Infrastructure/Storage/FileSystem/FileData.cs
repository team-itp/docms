using System;
using System.IO;
using System.Threading.Tasks;
using Docms.Domain.Documents;

namespace Docms.Infrastructure.Storage.FileSystem
{
    public class FileData : IData
    {
        private readonly FileInfo _fileInfo;
        private readonly Lazy<string> _hash;

        public FileData(string key, string filepath)
        {
            StorageKey = key;
            _fileInfo = new FileInfo(filepath);
            _hash = new Lazy<string>(() =>
            {
                using (var fs = _fileInfo.OpenRead())
                {
                    return Storage.Hash.CalculateHash(fs);
                }
            });
        }

        public string StorageKey { get; }

        public long Length => _fileInfo.Length;

        public string Hash => _hash.Value;

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(_fileInfo.OpenRead());
        }
    }
}
