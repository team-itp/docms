using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.LocalStorage
{
    public interface ILocalFileStorage
    {
        Task Create(PathString path, Stream stream, DateTime created, DateTime lastModified);
        Task Update(PathString path, Stream stream, DateTime created, DateTime lastModified);
        void MoveDocument(PathString originalPath, PathString destinationPath);
        void Delete(PathString path);
        FileInfo TempCopy(PathString path);
        bool FileExists(PathString path);
        long GetLength(PathString localFilePath);
        FileStream OpenRead(PathString path);
        DateTime GetCreated(PathString localFilePath);
        DateTime GetLastModified(PathString localFilePath);
        IEnumerable<PathString> GetFiles(PathString path);
        IEnumerable<PathString> GetDirectories(PathString path);
    }
}