using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public interface ILocalFileStorage
    {
        Task Create(string path, Stream stream, DateTime created, DateTime lastModified);
        Task Update(string path, Stream stream, DateTime lastModified);
        void MoveDocument(string originalPath, string destinationPath);
        void Delete(string path);
        FileInfo GetFile(string path);
        string CalculateHash(string path);
        IEnumerable<string> GetFiles(string path);
        IEnumerable<string> GetDirectories(string path);
    }
}