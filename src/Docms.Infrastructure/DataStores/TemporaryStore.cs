using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.DataStores
{
    public class TemporaryStore : ITemporaryStore
    {
        public async Task<ITempData> CreateAsync(Stream stream, long sizeOfStream)
        {
            if (sizeOfStream == -1)
            {
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return new InMemoryTempData(ms.ToArray(), ms.Length);
            }
            else
            {
                MemoryStream ms = new MemoryStream((int)sizeOfStream);
                await stream.CopyToAsync(ms);
                return new InMemoryTempData(ms.ToArray(), sizeOfStream);
            }
        }

        public Task DisposeAsync(ITempData data)
        {
            data.Dispose();
            return Task.CompletedTask;
        }
    }

    public sealed class InMemoryTempData : ITempData
    {
        private bool disposedValue;
        private byte[] data;

        public InMemoryTempData(byte[] data, long SizeOfStream)
        {
            SizeOfData = SizeOfStream;
            this.data = data;
        }

        public long SizeOfData { get; }

        public Task<Stream> OpenStreamAsync()
        {
            if (disposedValue)
            {
                throw new InvalidOperationException();
            }
            return Task.FromResult<Stream>(new MemoryStream(data));
        }

        public void Dispose()
        {
            data = null;
            disposedValue = true;
        }
    }
}
