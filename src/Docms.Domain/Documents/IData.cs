using System.IO;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IData
    {
        long Length { get; }
        string Hash { get; }
        Task<Stream> OpenStreamAsync();
    }
}
