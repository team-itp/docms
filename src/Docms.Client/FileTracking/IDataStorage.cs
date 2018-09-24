using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileTracking
{
    public interface IDataStorage
    {
        Task SaveAsync(Guid id, Stream stream);
        Task<Stream> OpenStreamAsync(Guid id);
        Task<long> GetSizeAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}