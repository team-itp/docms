using Docms.Client.Types;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.FileSystem
{
    public class LocalDirectoryInfo : IDirectoryInfo
    {
        public LocalDirectoryInfo(PathString path)
        {
            Path = path;
        }

        public PathString Path { get; }
    }
}