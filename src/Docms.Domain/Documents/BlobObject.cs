using System.IO;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public class BlobObject : IObject
    {
        private IBlobEntry _entry;

        public BlobObject(Hash hash, IBlobStorage storage)
        {
            Hash = hash;
            _entry = storage?.FetchBlobEntry(Hash);
        }

        public Hash Hash { get; }
        public long Size => _entry.Size;
        public Task<Stream> OpenAsync() => _entry.OpenAsync();
    }
}
