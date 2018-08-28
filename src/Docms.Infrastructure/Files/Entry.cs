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

        public Directory Parent => Path.DirectoryPath == null ? null : new Directory(Path.DirectoryPath, Storage);
    }

    public class Directory : Entry
    {
        internal Directory(FilePath path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<IEnumerable<Entry>> GetEntriesAsync()
        {
            return Storage.GetEntriesAsync(this);
        }

        public Task<File> SaveAsync(string filename, string contentType, Stream stream)
        {
            return Storage.SaveAsync(this, filename, contentType, stream);
        }
    }

    public class File : Entry
    {
        internal File(FilePath path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<Stream> OpenAsync()
        {
            return Storage.OpenAsync(this);
        }
    }
}
