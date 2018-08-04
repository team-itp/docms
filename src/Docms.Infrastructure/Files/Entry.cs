using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public abstract class Entry
    {
        protected IFileStorage Storage { get; }
        public FilePath Path { get; }

        protected Entry(string path, IFileStorage storage)
        {
            Storage = storage;
            Path = new FilePath(path);
        }
    }

    public class Directory : Entry
    {
        internal Directory(string path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<IEnumerable<Entry>> GetFilesAsync()
        {
            return Storage.GetFilesAsync(Path.ToString());
        }
    }

    public class File : Entry
    {
        internal File(string path, IFileStorage storage) : base(path, storage)
        {
        }

        public Task<FileProperties> GetPropertiesAsync()
        {
            return Storage.GetPropertiesAsync(Path.ToString());
        }

        public Task<Stream> OpenAsync()
        {
            return Storage.OpenAsync(Path.ToString());
        }

        public Task DeleteAsync()
        {
            return Storage.DeleteAsync(Path.ToString());
        }
    }
}
