using System.IO;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IBlobEntry
    {
        long Size { get; }
        Task<Stream> OpenAsync();
    }
}
