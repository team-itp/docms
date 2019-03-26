using Docms.Client.FileSystem;
using Docms.Client.Types;
using System.Collections.Generic;

namespace Docms.Client.Tests.Utils
{
    class MockDirectoryInfo : IDirectoryInfo
    {
        public MockDirectoryInfo(PathString path)
        {
            this.Path = path;
        }

        public PathString Path { get; }
        public List<IDirectoryInfo> Directories { get; } = new List<IDirectoryInfo>();
        public List<IFileInfo> Files { get; } = new List<IFileInfo>();
    }
}
