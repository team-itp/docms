using Docms.Client.FileTracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDataStorage : IDataStorage
    {
        private static Dictionary<Guid, byte[]> dataDictionary = new Dictionary<Guid, byte[]>();

        public async Task SaveAsync(Guid id, Stream data)
        {
            var ms = new MemoryStream();
            await data.CopyToAsync(ms);
            lock (dataDictionary)
            {
                if (dataDictionary.TryGetValue(id, out var value))
                {
                    dataDictionary[id] = ms.ToArray();
                }
                else
                {
                    dataDictionary.Add(id, ms.ToArray());
                }
            }
        }

        public Task<Stream> OpenStreamAsync(Guid id)
        {
            var ms = dataDictionary.TryGetValue(id, out var data) ? new MemoryStream(data) : null;
            return Task.FromResult(ms as Stream);
        }

        public Task<long> GetSizeAsync(Guid id)
        {
            return Task.FromResult(dataDictionary.TryGetValue(id, out var data) ? data.LongLength : -1);
        }

        public Task DeleteAsync(Guid id)
        {
            lock (dataDictionary)
            {
                dataDictionary.Remove(id);
            }
            return Task.CompletedTask;
        }
    }
}
