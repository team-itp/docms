using System.IO;

namespace Docms.Domain.Documents
{
    public interface IData
    {
        long Size { get; }
        Stream Open();
    }
}
