using System;

namespace Docms.Infrastructure.Files
{
    public class File
    {
        public FilePath Path { get; internal set; }
        public string ContentType { get; internal set; }
        public long Size { get; internal set; }
        public byte[] Sha1Hash { get; internal set; }
        public DateTimeOffset Created { get; internal set; }
        public DateTimeOffset LastModified { get; internal set; }
    }
}
