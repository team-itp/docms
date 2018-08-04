using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public interface IFileStorage
    {
        Task<IEnumerable<Entry>> GetFilesAsync(string path);
        Task<FileProperties> GetPropertiesAsync(string path);
        Task<Stream> OpenAsync(string path);
        Task<FileProperties> SaveAsync(string path, Stream stream);
        Task DeleteAsync(string path);
    }
}
