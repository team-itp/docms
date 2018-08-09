using System;

namespace Docms.Infrastructure.Files
{
    public class FileProperties
    {
        public File File { get; internal set; }
        public string ContentType { get; internal set; }
        public long Size { get; internal set; }
        public byte[] Hash { get; internal set; }
        public DateTimeOffset Created { get; internal set; }
        public DateTimeOffset LastModified { get; internal set; }
    }
}
