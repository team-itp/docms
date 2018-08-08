using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public abstract class Entry
    {
        protected IFileStorage Storage { get; }
        public FilePath Path { get; }

        protected Entry(FilePath path, IFileStorage storage)
        {
            Storage = storage;
            Path = path;
        }

        public Entry Parent => Path.DirectoryPath == null ? null : new Directory(Path.DirectoryPath, Storage);
    }

    public class Directory : Entry
    {
        internal Directory(FilePath path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<IEnumerable<Entry>> GetFilesAsync()
        {
            return Storage.GetFilesAsync(this);
        }

        public Task<FileProperties> SaveAsync(string filename, Stream stream)
        {
            return Storage.SaveAsync(this, filename, stream);
        }
    }

    public class File : Entry
    {
        internal File(FilePath path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<FileProperties> GetPropertiesAsync()
        {
            return Storage.GetPropertiesAsync(this);
        }

        public Task<Stream> OpenAsync()
        {
            return Storage.OpenAsync(this);
        }
    }
}
