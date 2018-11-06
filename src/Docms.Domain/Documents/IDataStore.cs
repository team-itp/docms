using System.IO;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IDataStore
    {
        string CreateKey();
        Task<IData> CreateAsync(string key, Stream stream);
        Task<IData> CreateAsync(string key, Stream stream, long sizeOfStream);
        Task<IData> FindAsync(string key);
        Task DeleteAsync(string key);
    }
}
