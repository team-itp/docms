using Docms.Domain.Documents;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.InMemory
{
    public class InMemoryData : IData
    {
        private readonly byte[] _data;
        private readonly Lazy<string> _hash;

        public InMemoryData(string key, byte[] data)
        {
            StorageKey = key;
            _data = data;
            _hash = new Lazy<string>(() => Storage.Hash.CalculateHash(data));
        }

        public string StorageKey { get; }
        public long Length => _data.Length;
        public string Hash => _hash.Value;

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(_data));
        }

        public static InMemoryData Create(string key, Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return Create(key, ms.ToArray());
            }
            ms = new MemoryStream();
            stream.CopyTo(ms);
            return Create(key, ms.ToArray());
        }

        public static InMemoryData Create(string key, byte[] data)
        {
            return new InMemoryData(key, data);
        }
    }
}
