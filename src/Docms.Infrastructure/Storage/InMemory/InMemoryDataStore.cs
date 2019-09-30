using Docms.Domain.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage.InMemory
{
    public class InMemoryDataStore : IDataStore
    {
        private readonly Dictionary<string, InMemoryData> values = new Dictionary<string, InMemoryData>();

        public string CreateKey()
        {
            var time = DateTime.UtcNow;
            var guid = Guid.NewGuid();
            return string.Format("{0:yyyyMMddHH}/{1}", time, guid);
        }

        public Task<IData> CreateAsync(string key, Stream stream)
        {
            var data = InMemoryData.Create(key, stream);
            values.Add(key, data);
            return Task.FromResult<IData>(data);
        }

        public Task<IData> CreateAsync(string key, Stream stream, long sizeOfStream)
        {
            return CreateAsync(key, stream);
        }

        public Task<IData> CreateAsync(byte[] byteArray)
        {
            var key = CreateKey();
            var data = InMemoryData.Create(key, byteArray);
            values.Add(key, data);
            return Task.FromResult<IData>(data);
        }

        public Task<IData> FindAsync(string key)
        {
            return Task.FromResult(values.TryGetValue(key, out var data) ? data : default(IData));
        }

        public Task DeleteAsync(string key)
        {
            values.Remove(key);
            return Task.CompletedTask;
        }

        public IEnumerable<string> ListAllKeys()
        {
            return values.Keys;
        }
    }
}
