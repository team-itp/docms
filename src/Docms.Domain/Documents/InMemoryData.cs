using System.IO;

namespace Docms.Domain.Documents
{
    public class InMemoryData : IData
    {
        private byte[] _data;

        public InMemoryData(byte[] data)
        {
            _data = data;
        }

        public byte[] Data => (byte[])_data.Clone();

        public long Size => _data.Length;

        public Stream Open()
        {
            return new MemoryStream(_data);
        }
    }
}
