using Docms.Client.FileSystem;
using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Docms.Client.Tests.Utils
{
    class MockFileSystem : IFileSystem
    {
        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            throw new NotImplementedException();
        }

        public IDirectoryInfo GetDirectoryInfo(PathString dirpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(PathString path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            throw new NotImplementedException();
        }
    }
}
