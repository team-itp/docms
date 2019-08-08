using System.IO;

namespace Docms.Domain.Documents
{
    public class BlobObject : ObjectBase
    {
        private IData _data;

        public BlobObject(IData data) : base(Hash.CalculateHash(data.Open()))
        {
            _data = data;
        }

        public long Size => _data.Size;
        public Stream Open() => _data.Open();
    }
}
