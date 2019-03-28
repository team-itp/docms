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
        DateTime LastModified { get; }
        Stream OpenRead();
        string CalculateHash();
    }
}
