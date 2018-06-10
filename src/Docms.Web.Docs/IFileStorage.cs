using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public interface IFileStorage
    {
        Task<DocumentFileInfo> GetFileInfoAsync(string path);
        Task<DocumentFileInfo> SaveAsync(string path, Stream stream);
    }
}