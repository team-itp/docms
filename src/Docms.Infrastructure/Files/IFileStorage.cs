using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public interface IFileStorage
    {
        Task<Entry> GetEntryAsync(string path);
        Task<Entry> GetEntryAsync(FilePath path);
        Task<Directory> GetDirectoryAsync(string path);
        Task<Directory> GetDirectoryAsync(FilePath path);
        Task<IEnumerable<Entry>> GetFilesAsync(Directory dir);
        Task<Stream> OpenAsync(File file);
        Task<File> SaveAsync(Directory dir, string filename, string contentType, Stream stream);
        Task MoveAsync(FilePath originalPath, FilePath destinationPath);
        Task DeleteAsync(Entry entry);
    }
}
