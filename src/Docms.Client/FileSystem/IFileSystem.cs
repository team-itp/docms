using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<PathString> GetDirectories(PathString path);
        IEnumerable<PathString> GetFiles(PathString path);
        IFileInfo GetFileInfo(PathString path);
        IDirectoryInfo GetDirectoryInfo(PathString dirpath);
        Task CreateDirectory(PathString path);
        Task CreateFile(PathString path, Stream stream, DateTime created, DateTime lastModified);
        Task Move(PathString fromPath, PathString toPath);
        Task Delete(PathString path);
    }
}
