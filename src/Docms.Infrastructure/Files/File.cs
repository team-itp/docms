using System;

namespace Docms.Infrastructure.Files
{
    public class File
    {
        public FilePath Path { get; set; }
        public long Size { get; set; }
        public byte[] Sha1Hash { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid CreatedUserId { get; set; }
    }
}
