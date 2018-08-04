using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public interface IFileStorage
    {
        Task<IEnumerable<Entry>> GetFilesAsync(string path);
        Task<FileInfo> GetFileAsync(string path);
        Task<Stream> OpenAsync(string path);
        Task<FileInfo> SaveAsync(string path, Stream stream);
    }
}
