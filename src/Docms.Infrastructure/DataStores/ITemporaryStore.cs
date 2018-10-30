using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.DataStores
{
    public interface ITemporaryStore
    {
        Task<ITempData> CreateAsync(Stream stream, long sizeOfStream);
        Task DisposeAsync(ITempData data);
    }

    public interface ITempData : IDisposable
    {
        long SizeOfData { get; }
        Task<Stream> OpenStreamAsync();
    }
}