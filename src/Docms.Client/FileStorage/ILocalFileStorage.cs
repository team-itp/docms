using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public interface ILocalFileStorage
    {
        Task Create(string path, Stream stream, DateTimeOffset created, DateTimeOffset lastModified);
        Task Update(string path, Stream stream, DateTimeOffset lastModified);
        void MoveDocument(string originalPath, string destinationPath);
        void Delete(string path);
        FileInfo GetFile(string path);
        string CalculateHash(string path);
    }
}