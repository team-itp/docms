using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.DataStores
{
    public class TemporaryStore : ITemporaryStore
    {
        public async Task<ITempData> CreateAsync(Stream stream, long sizeOfStream)
        {
            if (sizeOfStream > -1 && sizeOfStream < 33_554_432)
            {
                return await InMemoryTempData.Create(stream, sizeOfStream);
            }
            else
            {
                return await FileTempData.Create(stream);
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

        public InMemoryTempData(byte[] data)
        {
            SizeOfData = data.Length;
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

        internal static async Task<ITempData> Create(Stream stream, long sizeOfStream)
        {
            MemoryStream ms = new MemoryStream((int)sizeOfStream);
            await stream.CopyToAsync(ms);
            return new InMemoryTempData(ms.ToArray());
        }
    }

    public sealed class FileTempData : ITempData
    {
        private bool disposedValue;
        private FileInfo fileInfo;
        private List<Stream> streams = new List<Stream>();

        private FileTempData(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        internal static async Task<FileTempData> Create(Stream stream)
        {
            var tempPath = Path.GetTempFileName();
            var fi = new FileInfo(tempPath);
            using (var fs = fi.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
            return new FileTempData(fi);
        }

        public long SizeOfData => fileInfo.Length;

        public Task<Stream> OpenStreamAsync()
        {
            if (disposedValue)
            {
                throw new InvalidOperationException();
            }
            var stream = fileInfo.OpenRead();
            streams.Add(stream);
            return Task.FromResult<Stream>(stream);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                disposedValue = true;
                try
                {
                    streams.ForEach(s =>
                    {
                        s.Close();
                        s.Dispose();
                    });
                    streams.Clear();
                    fileInfo.Delete();
                }
                catch { }
            }
        }
    }
}
