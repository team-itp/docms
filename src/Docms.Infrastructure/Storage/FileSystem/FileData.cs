using System;
using System.IO;
using System.Threading.Tasks;
using Docms.Domain.Documents;

namespace Docms.Infrastructure.Storage.FileSystem
{
    public class FileData : IData
    {
        private FileInfo _fileInfo;
        private Lazy<string> _hash;

        public FileData(string filepath)
        {
            _fileInfo = new FileInfo(filepath);
            _hash = new Lazy<string>(() =>
            {
                using (var fs = _fileInfo.OpenRead())
                {
                    return Storage.Hash.CalculateHash(fs);
                }
            });
        }

        public long Length => _fileInfo.Length;

        public string Hash => _hash.Value;

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(_fileInfo.OpenRead());
        }
    }
}
