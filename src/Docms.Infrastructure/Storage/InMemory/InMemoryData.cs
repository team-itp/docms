using Docms.Domain.Documents;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.InMemory
{
    public class InMemoryData : IData
    {
        private byte[] _data;
        private Lazy<string> _hash;

        public InMemoryData(byte[] data)
        {
            _data = data;
            _hash = new Lazy<string>(() => Storage.Hash.CalculateHash(data));
        }

        public long Length => _data.Length;
        public string Hash => _hash.Value;

        public Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(_data));
        }

        public static InMemoryData Create(Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return Create(ms.ToArray());
            }
            ms = new MemoryStream();
            stream.CopyTo(ms);
            return Create(ms.ToArray());
        }

        public static InMemoryData Create(byte[] data)
        {
            return new InMemoryData(data);
        }
    }
}
