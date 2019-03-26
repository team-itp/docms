using Docms.Client.Types;
using System;
using System.IO;

namespace Docms.Client.FileSystem
{
    public interface IFileInfo
    {
        PathString Path { get; }
        long FileSize { get; }
        DateTime Created { get; }
        void SetCreated(DateTime created);
        DateTime LastModified { get; }
        void SetLastModified(DateTime lastModified);
        Stream OpenRead();
        Stream OpenWrite();
        string CalculateHash();
    }
}
