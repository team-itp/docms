using Docms.Client.Types;
using System;
using System.IO;

namespace Docms.Client.FileSystem
{
    public class LocalFileInfo : IFileInfo
    {
        private FileInfo fileInfo;

        public LocalFileInfo(PathString path, string fullpath)
        {
            fileInfo = new FileInfo(fullpath);
            Path = path;
        }

        public PathString Path { get; }
        public long FileSize => fileInfo.Length;
        public DateTime Created => fileInfo.CreationTimeUtc;
        public DateTime LastModified => fileInfo.LastWriteTimeUtc;
        public Stream OpenRead()
        {
            return fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream OpenWrite()
        {
            return fileInfo.Open(FileMode.Open, FileAccess.Write, FileShare.None);
        }
    }
}
