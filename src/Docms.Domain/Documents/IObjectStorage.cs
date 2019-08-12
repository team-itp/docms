using System.IO;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IObjectStorage
    {
        Task<IObject> FetchObjectAsync(Hash hash);
        Task SaveAsync(IObject obj);
    }
}
