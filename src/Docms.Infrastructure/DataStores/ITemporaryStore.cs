using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.DataStores
{
    public interface ITemporaryStore
    {
        Task<DateTime> SaveAsync(Guid id, Stream data);
        Task<Stream> OpenStreamAsync(Guid id);
        Task DeleteAsync(Guid id);
        Task DeleteBeforeAsync(DateTime timestamp);
    }
}