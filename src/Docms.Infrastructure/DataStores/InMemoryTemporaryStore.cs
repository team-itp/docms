using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.DataStores
{
    public class InMemoryTemporaryStore : ITemporaryStore
    {
        private static Dictionary<Guid, byte[]> dataDictionary = new Dictionary<Guid, byte[]>();
        private static Dictionary<Guid, DateTime> timestampDictionary = new Dictionary<Guid, DateTime>();

        public async Task<DateTime> SaveAsync(Guid id, Stream data)
        {
            var timestamp = DateTime.UtcNow;
            var ms = new MemoryStream();
            await data.CopyToAsync(ms);
            lock (dataDictionary)
            {
                dataDictionary.Add(id, ms.ToArray());
                timestampDictionary.Add(id, timestamp);
            }
            return timestamp;
        }

        public Task<Stream> OpenStreamAsync(Guid id)
        {
            var ms = dataDictionary.TryGetValue(id, out var data) ? new MemoryStream(data) : null;
            return Task.FromResult(ms as Stream);
        }

        public Task<int> GetFileSizeAsync(Guid id)
        {
            return Task.FromResult(dataDictionary.TryGetValue(id, out var data) ? data.Length : -1);
        }

        public Task DeleteAsync(Guid id)
        {
            lock (dataDictionary)
            {
                dataDictionary.Remove(id);
                timestampDictionary.Remove(id);
            }
            return Task.CompletedTask;
        }

        public Task DeleteBeforeAsync(DateTime timestamp)
        {
            foreach (var kv in timestampDictionary
                .Where((kv) => kv.Value <= timestamp)
                .ToArray())
            {
                DeleteAsync(kv.Key);
            }
            return Task.CompletedTask;
        }
    }
}
