using Docms.Client.Types;
using System.IO;

namespace Docms.Client.FileSystem
{
    public class LocalDirectoryInfo : IDirectoryInfo
    {
        private DirectoryInfo directoryInfo;

        public LocalDirectoryInfo(PathString path, string fullpath)
        {
            Path = path;
            directoryInfo = new DirectoryInfo(fullpath);
        }

        public PathString Path { get; }
    }
}