using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public interface ILocalFileStorage
    {
        Task Create(PathString path, Stream stream, DateTime created, DateTime lastModified);
        Task Update(PathString path, Stream stream, DateTime lastModified);
        void MoveDocument(PathString originalPath, PathString destinationPath);
        void Delete(PathString path);
        FileInfo GetFile(PathString path);
        FileInfo TempCopy(PathString path);
        string CalculateHash(PathString path);
        IEnumerable<PathString> GetFiles(PathString path);
        IEnumerable<PathString> GetDirectories(PathString path);
    }
}