using Docms.Domain.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Storage
{
    public class InMemoryDataStore : IDataStore
    {
        private readonly Dictionary<string, InMemoryData> values = new Dictionary<string, InMemoryData>();

        public string CreateKey()
        {
            var time = DateTime.UtcNow;
            var guid = Guid.NewGuid();
            return string.Format("{0}/{1}", time.Ticks % TimeSpan.TicksPerMillisecond, guid);
        }

        public Task<IData> CreateAsync(string key, Stream stream)
        {
            var data = InMemoryData.Create(stream);
            values.Add(key, data);
            return Task.FromResult<IData>(data);
        }

        public Task<IData> CreateAsync(string key, Stream stream, long sizeOfStream)
        {
            return CreateAsync(key, stream);
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
    }
}
