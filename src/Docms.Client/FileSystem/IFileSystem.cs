using Docms.Client.Types;
using System.Collections.Generic;

namespace Docms.Client.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<PathString> GetDirectories(PathString path);
        IEnumerable<PathString> GetFiles(PathString path);
        IFileInfo GetFileInfo(PathString path);
        IDirectoryInfo GetDirectoryInfo(PathString dirpath);
    }
}
