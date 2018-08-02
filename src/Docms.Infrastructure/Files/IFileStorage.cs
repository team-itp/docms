using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public interface IFileStorage
    {
        Task<IEnumerable<FilePath>> GetFilesAsync(string path);
        Task<File> GetFileAsync(string path);
        Task<Stream> OpenAsync(string path);
        Task<File> SaveAsync(string path, Stream stream);
    }
}
